using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.Events;

using Common;
using Common.Harmony;
using Common.Reflection;

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

	static class PrawnSuitLightsToggle // mostly from RandyKnapp's PrawnSuitLightSwitch mod
	{
		[HarmonyPatch(typeof(Exosuit), "Awake")]
		static class Exosuit_Awake_Patch
		{
			static bool Prepare() => Main.config.gameplayPatches;

			static void Postfix(Exosuit __instance)
			{
				var toggleLights = __instance.gameObject.ensureComponent<ToggleLights>();
				var toggleLightsPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth").GetComponent<SeaMoth>().toggleLights;

				toggleLights.copyFieldsFrom(toggleLightsPrefab, "lightsOnSound", "lightsOffSound", "onSound", "offSound", "energyPerSecond");

				toggleLights.lightsParent = __instance.transform.Find("lights_parent").gameObject;
				toggleLights.energyMixin = __instance.GetComponent<EnergyMixin>();
			}
		}

		[HarmonyPatch(typeof(Exosuit), "Update")]
		static class Exosuit_Update_Patch
		{
			static bool Prepare() => Main.config.gameplayPatches;

			static void Postfix(Exosuit __instance)
			{
				if (__instance.GetComponent<ToggleLights>() is ToggleLights toggleLights)
				{
					toggleLights.UpdateLightEnergy();

					if (__instance.GetPilotingMode() && Input.GetKeyDown(Main.config.toggleLightKey) && !(Player.main.GetPDA().isOpen || !AvatarInputHandler.main.IsEnabled()))
						toggleLights.SetLightsActive(!toggleLights.lightsActive);
				}
			}
		}
	}

	// if vehicle's health too low it will take random additional damage
	static class VehicleLowHealthExtraDamage
	{
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

		[HarmonyPatch(typeof(Vehicle), "Awake")]
		static class Vehicle_Awake_Patch
		{
			static bool Prepare() => Main.config.gameplayPatches;
			static void Postfix(Vehicle __instance) => __instance.gameObject.ensureComponent<LowHealthExtraDamage>();
		}
	}


	// Hide extra quick slots in vehicles
	// Modules installed in these slots working as usual
	// Intended for passive modules, issues with selectable modules
	static class VehiclesLessQuickSlots
	{
		static int _seamoth(Vehicle vehicle) => Math.Min(vehicle.slotIDs.Length, Main.config.maxSlotsCountSeamoth);
		static int _prawn(Vehicle vehicle)   => Math.Min(vehicle.slotIDs.Length, Main.config.maxSlotsCountPrawnSuit + 2);

		static CIEnumerable transpiler(CIEnumerable cins, bool isSeamoth)
		{
			MethodInfo maxSlotsCount = typeof(VehiclesLessQuickSlots).method(isSeamoth? nameof(_seamoth): nameof(_prawn));
			var list = cins.ToList();

			CIHelper.ciRemove(list, 0, 5);
			CIHelper.ciInsert(list, 0, CIHelper.toCIList(OpCodes.Ldarg_0, OpCodes.Call, maxSlotsCount, OpCodes.Stloc_0));

			return list;
		}

		[HarmonyPatch(typeof(Vehicle), "GetSlotBinding", new Type[] {})]
		static class Vehicle_GetSlotBinding_Patch
		{
			static bool Prepare() => Main.config.gameplayPatches;
			static CIEnumerable Transpiler(CIEnumerable cins) => transpiler(cins, true);
		}

		[HarmonyPatch(typeof(Exosuit), "GetSlotBinding", new Type[] {})]
		static class Exosuit_GetSlotBinding_Patch
		{
			static bool Prepare() => Main.config.gameplayPatches;
			static CIEnumerable Transpiler(CIEnumerable cins) => transpiler(cins, false);
		}
	}


	// get access to seamoth torpedo tubes when docked in moonpool
	[HarmonyPatch(typeof(SeaMoth), "OnDockedChanged")]
	static class SeaMoth_OnDockedChanged_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(SeaMoth __instance, Vehicle.DockType dockType)
		{
			foreach (var silo in new string[] {"TorpedoSiloLeft", "TorpedoSiloRight"})
				__instance.transform.Find(silo)?.gameObject.SetActive(dockType != Vehicle.DockType.Cyclops);
		}
	}


	// Fix hatch and antennas for docked vehicles in cyclops
	// Playing vehicle dock animation after load, dont find another way
	// Exosuit is also slightly moved from cyclops dock bay hatch, need to play all docking animations to fix it (like in moonpool)
	static class CyclopsDockingVehiclesFix
	{
		[HarmonyPatch(typeof(Vehicle), "Start")]
		static class Vehicle_Start_Patch
		{
			const float time = 7f;

			static bool Prepare() => Main.config.gameplayPatches;

			static void Postfix(Vehicle __instance)
			{
				if (!__instance.docked)
					return;

				SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
				if (subRoot != null && !subRoot.isBase) // we're docked in cyclops
				{
					(__instance as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", true);

					__instance.gameObject.callAfterDelay(time, new UnityAction(() =>
					{
						(__instance as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", false);
					}));
				}
			}
		}
	}

	// fix for loading inside the vehicle (https://github.com/Remodor/Subnautica_Mods/blob/master/Rm_VehicleLoadFix/src/patcher/VehiclePatcher.cs)
	[HarmonyPatch(typeof(Vehicle), "Start")]
	static class Vehicle_Start_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(Vehicle __instance)
		{
			if (__instance.pilotId != null && UniqueIdentifier.TryGetIdentifier(__instance.pilotId, out UniqueIdentifier pilotID))
				__instance.EnterVehicle(pilotID.GetComponent<Player>(), true, false);
		}
	}
}