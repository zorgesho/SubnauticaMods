using System.Collections.Generic;
using UnityEngine;

using SMLHelper.V2.Crafting;

using Common.CraftHelper;

namespace PrawnSuitGrapplingArmUpgrade
{
	class GrapplingArmUpgraded: MonoBehaviour {} // just to distinguish between vanilla arm and upgraded

	class GrapplingArmUpgradeModule: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		GrapplingArmUpgradeModule(): base(nameof(GrapplingArmUpgradeModule)) {}

		public static void patch()
		{
			if (TechType == 0)
				new GrapplingArmUpgradeModule().patchMe();
		}

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.ExosuitGrapplingArmModule, 1),
			new Ingredient(TechType.Polyaniline, 2),
			new Ingredient(TechType.Lithium, 2),
			new Ingredient(TechType.AramidFibers, 1),
			new Ingredient(TechType.AluminumOxide, 1),
		}};

		protected override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.ExosuitGrapplingArmModule));
			prefab.name = ClassID;

			return prefab;
		}

		void patchMe()
		{
			TechType = register("Prawn suit grappling arm MK2", "[todo description]", TechType.ExosuitGrapplingArmModule);

			setPDAGroup(TechGroup.Workbench, TechCategory.Workbench);
			addToCraftingNode(CraftTree.Type.Workbench, "ExosuitMenu");
			setEquipmentType(EquipmentType.ExosuitArm, QuickSlotType.Selectable);
			setBackgroundType(CraftData.BackgroundType.ExosuitArm);
			unlockOnStart();
		}
	}
}