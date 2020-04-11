using UnityEngine;
using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace PrawnSuitGrapplingArmUpgrade
{
	class GrapplingArmUpgraded: MonoBehaviour {} // just to distinguish between vanilla arm and upgraded

	class GrapplingArmUpgradeModule: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData
		(
			new Ingredient(TechType.ExosuitGrapplingArmModule, 1),
			new Ingredient(TechType.Polyaniline, 2),
			new Ingredient(TechType.Lithium, 2),
			new Ingredient(TechType.AramidFibers, 1),
			new Ingredient(TechType.AluminumOxide, 1)
		)	{ craftAmount = 1};

		public override GameObject getGameObject()
		{
			var prefab = CraftHelper.Utils.prefabCopy(TechType.ExosuitGrapplingArmModule);

			CraftHelper.Utils.initVFXFab(prefab, posOffset: new Vector3(0.08f, 0.1f, 0f), localMaxY: 0.35f, scaleFactor: 0.5f);

			return prefab;
		}

		public override void patch()
		{
			TechType = register("Prawn suit grappling arm MK2", "Enhances various parameters of grappling arm.", TechType.ExosuitGrapplingArmModule);

			addToGroup(TechGroup.Workbench, TechCategory.Workbench);
			addCraftingNodeTo(CraftTree.Type.Workbench, "ExosuitMenu");
			setEquipmentType(EquipmentType.ExosuitArm, QuickSlotType.Selectable);
			setBackgroundType(CraftData.BackgroundType.ExosuitArm);

			if (Main.config.fragmentCountToUnlock > 0)
				setFragmentToUnlock(TechType.ExosuitGrapplingArmFragment, Main.config.fragmentCountToUnlock, 7f);
			else
				setTechTypeForUnlock(TechType.ExosuitGrapplingArmModule);
		}
	}
}