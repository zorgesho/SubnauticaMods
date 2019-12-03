using System.Collections.Generic;
using Harmony;
using Common;

namespace MiscPrototypes
{
	class PickableStorageManager
	{
		public static int getItemsCount(TechType techType)
		{
			//PickupableStorage[] array = GameObject.FindObjectsOfType<PickupableStorage>();

			int count = 0;
			//for (int i = 0; i < array.Length; i++)
			//	count += array[i].storageContainer.container.GetCount(techType);

			List<InventoryItem> list0 = new List<InventoryItem>();
		
			Inventory.main._container.GetItems(TechType.LuggageBag, list0);
			Inventory.main._container.GetItems(TechType.SmallStorage, list0);

			for (int i = 0; i < list0.Count; i++)
			{
				$"bag: {list0[i]} , {list0[i].item}".onScreen();

				PickupableStorage p = list0[i].item.gameObject.GetComponentInChildren<PickupableStorage>();
				if (p)
				{
					count += p.storageContainer.container.GetCount(techType);
				}
				else
					"pickable is null".onScreen();
			}

			//if (Inventory.main._container.GetCount(TechType.LuggageBag) > 0 || Inventory.main._container.GetCount(TechType.SmallStorage) > 0)
			//{
			//	Inventory.main
			//}

			return count;
		}
	}
	
	
	[HarmonyPatch(typeof(Inventory), "GetPickupCount")]
	static class Inventory_GetPickupCount_Patch
	{
		static bool Prefix(Inventory __instance, TechType pickupType, ref int __result)
		{
			// HACK: exosuit checked every frame, constructor checked every second, no need check storages for these
			if (pickupType == TechType.Exosuit || pickupType == TechType.Constructor)
				return true;

			__result = __instance._container.GetCount(pickupType) + PickableStorageManager.getItemsCount(pickupType);

			return false;
		}
	}

	//[HarmonyPatch(typeof(Inventory), "DestroyItem")]
	//class Inventory_DestroyItem_Patch
	//{
	//	static bool Prefix(Inventory __instance, TechType destroyTechType, bool allowGenerics = false)
	//	{
	//		return false;
	//	}
	//}
}