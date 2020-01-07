using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace PrawnSuitSonarUpgrade
{
	class PrawnSonarModule: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData()	{ craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.SeamothSonarModule, 1),
			new Ingredient(TechType.WiringKit, 1),
			new Ingredient(TechType.ComputerChip, 1)
		}};

		public override GameObject getGameObject() => Object.Instantiate(CraftData.GetPrefabForTechType(TechType.VehicleArmorPlating));

		public override void patch()
		{
			TechType = register("Prawn suit sonar", "Seamoth sonar modified to use on prawn suit.", TechType.SeamothSonarModule);

			addToGroup(TechGroup.Workbench, TechCategory.Workbench);
			addCraftingNodeTo(CraftTree.Type.Workbench, "ExosuitMenu");
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.SelectableChargeable);

			setTechTypeForUnlock(TechType.SeamothSonarModule);
		}
	}
}