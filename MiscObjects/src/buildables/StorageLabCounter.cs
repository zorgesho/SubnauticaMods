using UnityEngine;

using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class StorageLabCounter: CraftableObject
	{
		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 4));

		public override void patch()
		{
			register("Counter", "Counter with drawers.", SpriteManager.Get(TechType.LabCounter));

			setPDAGroup(TechGroup.Miscellaneous, TechCategory.Misc, TechType.CoffeeVendingMachine);
			
			if (Main.config.removeVanillaCounter)
				CraftDataHandler.RemoveFromGroup(TechGroup.Miscellaneous, TechCategory.Misc, TechType.LabCounter);

			setTechTypeForUnlock(TechType.LabCounter);
		}

		protected override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.LabCounter));

			Common.Debug.dumpGameObject(prefab).saveToFile("counter");

			prefab.getOrAddComponent<TechTag>().type = TechType;
			prefab.GetComponent<PrefabIdentifier>().ClassId = ClassID;

			StorageHelper.addStorageToPrefab(prefab, "Open drawers", "DRAWERS", 7, 4);

			return prefab;
		}
	}
}