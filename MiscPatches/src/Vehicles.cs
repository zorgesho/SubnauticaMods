using System;
using System.Linq;
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

#if GAME_SN
	[PatchClass]
	static class PrawnSuitLightsToggle // mostly from RandyKnapp's PrawnSuitLightSwitch mod
	{
		static bool prepare() => Main.config.gameplayPatches;

		[HarmonyPostfix, HarmonyPatch(typeof(Exosuit), "Awake")]
		static void Exosuit_Awake_Postfix(Exosuit __instance)
		{
			var exoGO = __instance.gameObject;

			var toggleLights = exoGO.ensureComponent<ToggleLights>();
			var toggleLightsPrefab = Resources.Load<GameObject>("WorldEntities/Tools/SeaMoth").GetComponent<SeaMoth>().toggleLights;

			toggleLights.copyFieldsFrom(toggleLightsPrefab, "lightsOnSound", "lightsOffSound", "onSound", "offSound", "energyPerSecond");
			toggleLights.lightsParent = exoGO.getChild("lights_parent");
			toggleLights.energyMixin = __instance.GetComponent<EnergyMixin>();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Exosuit), "Update")]
		static void Exosuit_Update_Postfix(Exosuit __instance)
		{
			if (__instance.GetComponent<ToggleLights>() is ToggleLights toggleLights)
			{
				toggleLights.UpdateLightEnergy();

				if (__instance.GetPilotingMode() && Input.GetKeyDown(Main.config.toggleLightKey) && !(Player.main.GetPDA().isOpen || !AvatarInputHandler.main.IsEnabled()))
					toggleLights.SetLightsActive(!toggleLights.GetLightsActive());
			}
		}
	}
#endif

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
				!vehicle || (!vehicle.GetRecentlyUndocked() &&
#if GAME_SN
				!vehicle.precursorOutOfWater &&
#endif
				!vehicle.docked && !vehicle.IsInsideAquarium());

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

#if GAME_SN
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
			if (UniqueIdentifier.TryGetIdentifier(__instance.pilotId, out var pilotID))
				__instance.EnterVehicle(pilotID.GetComponent<Player>(), true, false);
		}
	}

	// hiding torpedoes from the tubes when they depleted
	// using torpedoes from lower storages (same torpedo type)
	[OptionalPatch, PatchClass]
	static class SeamothTorpedoesPatches
	{
		static bool prepare() => Main.config.gameplayPatches;

		static readonly string[] torpedoModels =
		{
			"Submersible_seaMoth_torpedo_silo_L1_geo1/Submersible_seaMoth_torpedo_L2_geo",
			"Submersible_seaMoth_torpedo_silo_R2_geo/Submersible_seaMoth_torpedo_R2_geo",
			"Submersible_seaMoth_torpedo_silo_L1_geo/Submersible_seaMoth_torpedo_L1_geo",
			"Submersible_seaMoth_torpedo_silo_R1_geo/Submersible_seaMoth_torpedo_R1_geo"
		};

		static void setTorpedoVisible(this SeaMoth seamoth, int slotID, bool visible) =>
			seamoth.gameObject.getChild("Model/Submersible_SeaMoth_extras/Submersible_seaMoth_geo/" + torpedoModels[slotID]).SetActive(visible);

		static int getSlotByTorpedoContainer(this SeaMoth seamoth, ItemsContainer container) =>
			Enumerable.Range(0, 4).Where(slotID => seamoth.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule) == container).DefaultIfEmpty(-1).First();

		static void updateTorpedoesVisibility(this SeaMoth seamoth)
		{
			if (!seamoth)
				return;

			for (int i = 0; i < 4; i++)
				if (seamoth.GetStorageInSlot(i, TechType.SeamothTorpedoModule) is ItemsContainer storage)
					seamoth.setTorpedoVisible(i, storage._items.Count > 0);
		}

		static int storageSlotOffset = -1; // for SlotExtender mod

		static ItemsContainer getStorageInSlot(this SeaMoth seamoth, int slotID)
		{
			if (storageSlotOffset == -1)
				storageSlotOffset = Type.GetType("SlotExtender.Configuration.SEConfig, SlotExtender")?.field("STORAGE_SLOTS_OFFSET").GetValue(null).convert<int>() ?? 0;

			var storage = seamoth.GetStorageInSlot(slotID, TechType.VehicleStorageModule);

			if (storage == null && storageSlotOffset > 0)
				storage = seamoth.GetStorageInSlot(slotID + storageSlotOffset, TechType.VehicleStorageModule);

			return storage;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Vehicle), "TorpedoShot")]
		static void Vehicle_TorpedoShot_Postfix(ItemsContainer container, TorpedoType torpedoType, bool __result)
		{
			if (!__result || !(container.tr.GetComponentInParent<SeaMoth>() is SeaMoth seamoth))
				return;

			int torpedoSlotID = seamoth.getSlotByTorpedoContainer(container);
			Common.Debug.assert(torpedoSlotID != -1);

			var storage = seamoth.getStorageInSlot((torpedoSlotID == 0 || torpedoSlotID == 2)? 0: 1); // using only lower storages
			var itemList = storage?.GetItems(torpedoType.techType); // using same torpedo type

			if (itemList?.Count > 0)
			{
				var item = itemList[0];
				storage.RemoveItem(item.item);
				container.AddItem(item.item);
			}

			seamoth.setTorpedoVisible(torpedoSlotID, container._items.Count > 0);
		}

		// patches for showing/hiding torpedoes during storage operations
		static readonly EventWrapper onAddItem = typeof(ItemsContainer).evnt("onAddItem").wrap();
		static readonly EventWrapper onRemoveItem = typeof(ItemsContainer).evnt("onRemoveItem").wrap();

		static SeaMoth currentSeamoth;

		static void handleStorageEvents(bool handle)
		{
			static void _updateStorage(InventoryItem _) => updateTorpedoesVisibility(currentSeamoth);

			if (!currentSeamoth)
				return;

			for (int i = 0; i < 4; i++)
			{
				if (!(currentSeamoth.GetStorageInSlot(i, TechType.SeamothTorpedoModule) is ItemsContainer storage))
					continue;

				if (handle)
				{
					onAddItem.add<OnAddItem>(storage, _updateStorage);
					onRemoveItem.add<OnRemoveItem>(storage, _updateStorage);
				}
				else
				{
					onAddItem.remove<OnAddItem>(storage, _updateStorage);
					onRemoveItem.remove<OnRemoveItem>(storage, _updateStorage);
				}
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(SeaMoth), "OpenTorpedoStorage")]
		static void SeaMoth_OpenTorpedoStorage_Postfix(SeaMoth __instance)
		{
			currentSeamoth = __instance;
			handleStorageEvents(true);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Inventory), "ClearUsedStorage")]
		static void Inventory_ClearUsedStorage_Postfix()
		{
			if (!currentSeamoth)
				return;

			handleStorageEvents(false);
			currentSeamoth = null;
		}
	}
#endif
}