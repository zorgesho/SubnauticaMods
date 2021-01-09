using Common.Crafting;

namespace PrawnSuitSonarUpgrade
{
	class PrawnSonarModule: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new TechInfo
		(
			new TechInfo.Ing(TechType.SeamothSonarModule),
			new TechInfo.Ing(TechType.WiringKit),
			new TechInfo.Ing(TechType.ComputerChip)
		);

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