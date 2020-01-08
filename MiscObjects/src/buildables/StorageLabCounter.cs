using UnityEngine;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class StorageLabCounter: CraftableObject
	{
		class L10n: LanguageHelper
		{
			public const string ids_CounterItem		= "Counter";
			public const string ids_CounterItemDesc	= "Counter with drawers.";

			public static string ids_DrawersInv	 = "DRAWERS";
			public static string ids_OpenDrawers = "Open drawers";
		}

		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 4));

		public override void patch()
		{
			register(L10n.ids_CounterItem, L10n.ids_CounterItemDesc, TechType.LabCounter);

			addToGroup(TechGroup.Miscellaneous, TechCategory.Misc, TechType.CoffeeVendingMachine);

			if (Main.config.removeVanillaCounter)
				CraftDataHandler.RemoveFromGroup(TechGroup.Miscellaneous, TechCategory.Misc, TechType.LabCounter);

			setTechTypeForUnlock(TechType.LabCounter);
		}

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.LabCounter));
			prefab.AddComponent<TechTag>(); // just in case

			StorageHelper.addStorageToPrefab(prefab, L10n.str("ids_OpenDrawers"), L10n.str("ids_DrawersInv"), 7, 4);

			return prefab;
		}
	}
}