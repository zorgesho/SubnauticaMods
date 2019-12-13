using System.Collections.Generic;
using UnityEngine;

using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace PrawnSuitGrapplingArmUpgrade
{
	class GrapplingArmUpgraded: MonoBehaviour {} // just to distinguish between vanilla arm and upgraded

	class GrapplingArmUpgradeModule: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.ExosuitGrapplingArmModule, 1),
			new Ingredient(TechType.Polyaniline, 2),
			new Ingredient(TechType.Lithium, 2),
			new Ingredient(TechType.AramidFibers, 1),
			new Ingredient(TechType.AluminumOxide, 1),
		}};

		public override GameObject getGameObject() => Object.Instantiate(CraftData.GetPrefabForTechType(TechType.ExosuitGrapplingArmModule));

		public override void patch()
		{
			TechType = register("Prawn suit grappling arm MK2", "Enhances various parameters of grappling arm.", SpriteManager.Get(TechType.ExosuitGrapplingArmModule));

			addToGroup(TechGroup.Workbench, TechCategory.Workbench);
			addCraftingNodeTo(CraftTree.Type.Workbench, "ExosuitMenu");
			setEquipmentType(EquipmentType.ExosuitArm, QuickSlotType.Selectable);
			setBackgroundType(CraftData.BackgroundType.ExosuitArm);
			setTechTypeForUnlock(TechType.ExosuitGrapplingArmModule);
		}
	}
}