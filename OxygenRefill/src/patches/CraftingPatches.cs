﻿using System;
using System.Linq;
using System.Collections.Generic;

using Harmony;

using Common;

namespace OxygenRefill
{
	using static OxygenTankUtils;
	using Object = UnityEngine.Object;

	// used in crafting
	[HarmonyPatch(typeof(Inventory), "DestroyItem")]
	static class Inventory_DestroyItem_Patch
	{
		static bool Prefix(Inventory __instance, TechType destroyTechType)
		{
			if (!isTankTechType(destroyTechType))
				return true;

			// first try to use tank in slot for crafting
			if (isTankAtSlot(destroyTechType))
			{
				InventoryItem item = __instance.equipment.RemoveItem("Tank", true, false);
				Object.Destroy(item.item.gameObject);

				return false;
			}

			// then search tank in inventory that is not full
			return !__instance._container.removeItem(destroyTechType, tank => !isTankFull(tank));
		}
	}

	// used for counting ingredients for crafting
	[HarmonyPatch(typeof(Inventory), "GetPickupCount")]
	static class Inventory_GetPickupCount_Patch
	{
		static bool Prefix(Inventory __instance, TechType pickupType, ref int __result)
		{
			if (!isTankTechType(pickupType))
				return true;

			// count not full tanks in inventory with that techtype
			__result = __instance._container.getItemsCount(pickupType, tank => !isTankFull(tank));

			if (isTankAtSlot(pickupType))
				__result++;

			return false;
		}
	}


	static class ItemsContainerExtensions
	{
		public static int getItemsCount(this ItemsContainer container, TechType techType, Predicate<Pickupable> predicate)
		{
			if (container._items.TryGetValue(techType, out ItemsContainer.ItemGroup itemGroup))
				return itemGroup.items.Count(i => predicate(i.item));
			else
				return 0;
		}

		public static bool removeItem(this ItemsContainer container, TechType techType, Predicate<Pickupable> predicate)
		{
			if (!container._items.TryGetValue(techType, out ItemsContainer.ItemGroup itemGroup))
				return false;

			List<InventoryItem> items = itemGroup.items;

			for (int i = items.Count - 1; i >= 0; i--)
			{
				InventoryItem inventoryItem = items[i];
				Pickupable item = inventoryItem.item;
				if (!(container as IItemsContainer).AllowedToRemove(item, true))
					return false;

				if (predicate(item))
				{
					items.RemoveAt(i);
					if (items.Count == 0)
						container._items.Remove(techType);

					inventoryItem.container = null;

					typeof(Pickupable).method("remove_onTechTypeChanged").Invoke(item, new object[] { (OnTechTypeChanged)container.UpdateItemTechType });

					container.count--;
					container.unsorted = true;
					container.NotifyRemoveItem(inventoryItem);

					Object.Destroy(item.gameObject);
					return true;
				}
			}

			return false;
		}
	}
}