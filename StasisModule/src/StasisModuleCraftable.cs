using Common.Crafting;

namespace StasisModule
{
	[CraftHelper.NoAutoPatch]
	class StasisModule: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new // TODO
		(
			new (TechType.Titanium),
			new (TechType.Gold)
		);

		protected override void initPrefabPool() => addPrefabToPool(TechType.ExosuitJetUpgradeModule);

		public override void patch()
		{
			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			unlockOnStart(); // TODO
		}
	}

#if GAME_SN
	class SeaMothStasisModule: StasisModule
	{
		public static new TechType TechType { get; private set; } = 0;

		public override void patch()
		{
			TechType = register(); // TODO
			base.patch();

			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "SeamothModules");
			setEquipmentType(EquipmentType.SeamothModule, QuickSlotType.Selectable);
		}
	}
#elif GAME_BZ
	class SeaTruckStasisModule: StasisModule
	{
		public static new TechType TechType { get; private set; } = 0;

		public override void patch()
		{
			TechType = register(); // TODO
			base.patch();

			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "SeaTruckUpgrade");
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/SeatruckUpgrades");
			setEquipmentType(EquipmentType.SeaTruckModule, QuickSlotType.Selectable);
		}
	}
#endif
	class PrawnSuitStasisModule: StasisModule
	{
		public static new TechType TechType { get; private set; } = 0;

		public override void patch()
		{
			TechType = register(); // TODO
			base.patch();

			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules");
#if GAME_BZ
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/ExosuitUpgrades");
#endif
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.Instant);
		}
	}
}