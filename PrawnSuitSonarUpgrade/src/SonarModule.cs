using SMLHelper.V2.Crafting;
using Common.Crafting;

namespace PrawnSuitSonarUpgrade
{
	class PrawnSonarModule: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData
		(
			new Ingredient(TechType.SeamothSonarModule, 1),
			new Ingredient(TechType.WiringKit, 1),
			new Ingredient(TechType.ComputerChip, 1)
		)	{ craftAmount = 1};

		protected override void initPrefabPool() => addPrefabToPool(TechType.VehicleArmorPlating);

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