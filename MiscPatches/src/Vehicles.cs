using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.Events;

using Common;
using Common.Harmony;

namespace MiscPatches
{
	using CIEnumerable = IEnumerable<CodeInstruction>;

	// turn off vehicle lights by default
	[HarmonyPatch(typeof(Vehicle), "Start")]
	static class Vehicle_Start_Patch_LightsOffByDefault
	{
		const float time = 1.2f;

		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(Vehicle __instance) =>
			__instance.gameObject.callAfterDelay(time, new UnityAction(() =>
			{
				if (__instance.gameObject.GetComponentInChildren<ToggleLights>() is ToggleLights lights)
				{
					// turn light off. Not using SetLightsActive because of sound
					lights.lightsActive = false;
					lights.lightsParent.SetActive(false);

					if (lights.energyPerSecond == 0f)
						lights.energyPerSecond = Main.config.vehicleLightEnergyPerSec;					$"light energy consumption for {__instance} is {lights.energyPerSecond}".logDbg();
				}
			}));
	}


	[PatchClass]
	static class PrawnSuitLightsToggle // mostly from RandyKnapp's PrawnSuitLightSwitch mod
	{
		static bool prepare() => Main.config.gameplayPatches;

		[HarmonyPostfix, HarmonyPatch(typeof(Exosuit), "Awake")]
		static void Exosuit_Awake_Postfix(Exosuit __instance)
		{
			var toggleLights = __instance.gameObject.ensureComponent<ToggleLights>();
			var toggleLightsPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth").GetComponent<SeaMoth>().toggleLights;

			toggleLights.copyFieldsFrom(toggleLightsPrefab, "lightsOnSound", "lightsOffSound", "onSound", "offSound", "energyPerSecond");

			toggleLights.lightsParent = __instance.transform.Find("lights_parent").gameObject;
			toggleLights.energyMixin = __instance.GetComponent<EnergyMixin>();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Exosuit), "Update")]
		static void Exosuit_Update_Postfix(Exosuit __instance)
		{
			if (__instance.GetComponent<ToggleLights>() is ToggleLights toggleLights)
			{
				toggleLights.UpdateLightEnergy();

				if (__instance.GetPilotingMode() && Input.GetKeyDown(Main.config.toggleLightKey) && !(Player.main.GetPDA().isOpen || !AvatarInputHandler.main.IsEnabled()))
					toggleLights.SetLightsActive(!toggleLights.lightsActive);
			}
		}
	}

	// vehicle will take random additional damage if its health is too low
	[PatchClass]
	static class VehicleLowHealthExtraDamage
	{
		static bool prepare() => Main.config.gameplayPatches;

		[HarmonyPostfix, HarmonyPatch(typeof(Vehicle), "Awake")]
		static void Vehicle_Awake_Postfix(Vehicle __instance) => __instance.gameObject.ensureComponent<LowHealthExtraDamage>();

		class LowHealthExtraDamage: MonoBehaviour
		{
			Vehicle vehicle;
			LiveMixin liveMixin;
			FMOD_CustomEmitter soundOnDamage;

			void Start()
			{
				liveMixin = GetComponent<LiveMixin>();
				vehicle = GetComponent<Vehicle>();
				soundOnDamage = GetComponent<CrushDamage>().soundOnDamage;

				InvokeRepeating(nameof(healthUpdate), 0f, Main.config.continuousDamageCheckInterval);
			}

			bool isCanTakeDamage() =>
				(!vehicle || (!vehicle.GetRecentlyUndocked() && !vehicle.docked && !vehicle.precursorOutOfWater && !vehicle.IsInsideAquarium()));

			void healthUpdate()
			{
				if (!gameObject.activeInHierarchy || !enabled)
					return;

				if (liveMixin.health / liveMixin.maxHealth < Main.config.minHealthPercentForContinuousDamage &&
					UnityEngine.Random.value < Main.config.chanceForDamage &&
					isCanTakeDamage())
				{
					liveMixin.TakeDamage(Main.config.additionalContinuousDamage, transform.position, DamageType.Pressure, null);
					soundOnDamage?.Play();																							$"LowHealthExtraDamage: {vehicle} health:{liveMixin.health}".logDbg();
				}
			}
		}
	}

	// Hide extra quick slots in vehicles
	// Modules installed in these slots working as usual
	// Intended for passive modules, issues with selectable modules
	[PatchClass]
	static class VehiclesLessQuickSlots
	{
		static bool prepare() => Main.config.gameplayPatches;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Vehicle), "GetSlotBinding", new Type[0])]
		[HarmonyPatch(typeof(Exosuit), "GetSlotBinding", new Type[0])]
		static CIEnumerable GetSlotBinding_Transpiler(CIEnumerable cins)
		{
			var list = cins.ToList();

			static int _slotCount(Vehicle vehicle)
			{
				int maxSlotCount = vehicle is SeaMoth? Main.config.maxSlotCountSeamoth: Main.config.maxSlotCountPrawnSuit + 2;
				return Math.Min(vehicle.slotIDs.Length, maxSlotCount);
			}

			list.ciRemove(0, 5);
			list.ciInsert(0,
				OpCodes.Ldarg_0,
				CIHelper.emitCall<Func<Vehicle, int>>(_slotCount),
				OpCodes.Stloc_0);

			return list;
		}
	}

	// get access to seamoth torpedo tubes when docked in moonpool
	[HarmonyPatch(typeof(SeaMoth), "OnDockedChanged")]
	static class SeaMoth_OnDockedChanged_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(SeaMoth __instance, Vehicle.DockType dockType)
		{
			foreach (var silo in new[] { "TorpedoSiloLeft", "TorpedoSiloRight" })
				__instance.transform.Find(silo)?.gameObject.SetActive(dockType != Vehicle.DockType.Cyclops);
		}
	}

	// fix for loading inside the vehicle (https://github.com/Remodor/Subnautica_Mods/blob/master/Rm_VehicleLoadFix/src/patcher/VehiclePatcher.cs)
	[HarmonyPatch(typeof(Vehicle), "Start")]
	static class Vehicle_Start_Patch_LoadingFix
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(Vehicle __instance)
		{
			if (__instance.pilotId != null && UniqueIdentifier.TryGetIdentifier(__instance.pilotId, out UniqueIdentifier pilotID))
				__instance.EnterVehicle(pilotID.GetComponent<Player>(), true, false);
		}
	}
}