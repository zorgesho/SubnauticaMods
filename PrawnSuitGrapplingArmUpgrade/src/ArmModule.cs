using System.Collections.Generic;
using UnityEngine;

using SMLHelper.V2.Crafting;

using Common.CraftHelper;

namespace PrawnSuitGrapplingArmUpgrade
{
	class GrapplingArmUpgradeModule: CraftableObject
	{
		new static public TechType TechType { get; private set; } = 0;

		GrapplingArmUpgradeModule(): base(nameof(GrapplingArmUpgradeModule)) {}

		static public void patch()
		{
			if (TechType == 0)
				new GrapplingArmUpgradeModule().patchMe();
		}

		override protected TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.ExosuitGrapplingArmModule, 1),
			new Ingredient(TechType.Peeper, 5),
		}};

		override protected GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.ExosuitGrapplingArmModule));
			prefab.name = ClassID;

			return prefab;
		}

		void patchMe()
		{
			TechType = register("Prawn suit grappling arm MK2", "Upgraded grappling arm", TechType.ExosuitGrapplingArmModule);

			setPDAGroup(TechGroup.Workbench, TechCategory.Workbench);
			addToCraftingNode(CraftTree.Type.Workbench, "ExosuitMenu");
			setEquipmentType(EquipmentType.ExosuitArm, QuickSlotType.Selectable);
			setBackgroundType(CraftData.BackgroundType.ExosuitArm);
		}
	}
}
