using Harmony;
using UnityEngine;
using UWE;

namespace OxygenRefill
{
	[HarmonyPatch(typeof(CraftData), "GetPrefabForTechType")]
	static class CraftData_GetPrefabForTechType_Patch
	{
		static bool Prefix(ref GameObject __result, TechType techType, bool verbose)
		{
			if (techType == OxygenRefillTechType.TankRefill)
			{
				__result = PrefabDatabase.GetPrefabForFilename("WorldEntities/Tools/Tank");
				return false;
			}
			if (techType == OxygenRefillTechType.DoubleTankRefill)
			{
				__result = PrefabDatabase.GetPrefabForFilename("WorldEntities/Tools/DoubleTank");
				return false;
			}
			if (techType == OxygenRefillTechType.PlasteelTankRefill)
			{
				__result = PrefabDatabase.GetPrefabForFilename("WorldEntities/Tools/PlasteelTank");
				return false;
			}
			if (techType == OxygenRefillTechType.HighCapacityTankRefill)
			{
				__result = PrefabDatabase.GetPrefabForFilename("WorldEntities/Tools/HighCapacityTank");
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Oxygen), "AddOxygen")]
	static class Oxygen_AddOxygen_Patch
	{
		static bool Prefix(Oxygen __instance, ref float __result, float amount)
		{
			float num = 0f;
			if (__instance.isPlayer)
			{
				num = Mathf.Min(amount, __instance.oxygenCapacity - __instance.oxygenAvailable);
				__instance.oxygenAvailable += num;
			}
			__result = num;
			return false;
		}
	}

	[HarmonyPatch(typeof(Oxygen), "Awake")]
	static class Oxygen_Awake_Patch
	{
		static bool Prefix(Oxygen __instance)
		{
			if (!__instance.isPlayer)
			{
				__instance.gameObject.AddComponent<OxygenAdvanced>();
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Vehicle), "ReplenishOxygen")]
	static class Vehicle_ReplenishOxygen_Patch
	{
		static bool Prefix(Vehicle __instance)
		{
			if (__instance.turnedOn && __instance.replenishesOxygen && __instance.GetPilotingMode() && __instance.CanPilot() && __instance.IsPowered())
			{
				Player.main.oxygenMgr.AddOxygen(__instance.oxygenPerSecond);
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(Inventory), "DestroyItem")]
	static class Inventory_DestroyItem_Patch
	{
		static bool Prefix(Inventory __instance, TechType destroyTechType, bool allowGenerics = false)
		{
			InventoryItem tankItem = Inventory.main.equipment.GetItemInSlot("Tank");
			if (tankItem != null && tankItem.item.GetTechType() == destroyTechType)
			{
				Oxygen oxygen = tankItem.item.GetComponent<Oxygen>();
				if (oxygen.oxygenAvailable < oxygen.oxygenCapacity)
				{
					InventoryItem item = Inventory.main.equipment.RemoveItem("Tank", true, false);
					UnityEngine.Object.Destroy(item.item.gameObject);

					return false;
				}
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(Inventory), "GetPickupCount")]
	static class Inventory_GetPickupCount_Patch
	{
		static bool Prefix(Inventory __instance, TechType pickupType, ref int __result)
		{
			InventoryItem tankItem = Inventory.main.equipment.GetItemInSlot("Tank");
			if (tankItem == null || tankItem.item.GetTechType() != pickupType)
				return true;

			__result = __instance._container.GetCount(pickupType) + 1;
			
			return false;
		}
	}
}