using Harmony;
using UnityEngine;

namespace OxygenRefill
{
	[HarmonyPatch(typeof(Oxygen), "AddOxygen")]
	static class Oxygen_AddOxygen_Patch
	{
		static bool Prefix(Oxygen __instance) => __instance.isPlayer;
	}


/*
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
*/
}