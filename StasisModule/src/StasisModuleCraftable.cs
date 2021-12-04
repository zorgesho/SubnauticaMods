using Common;
using Common.Crafting;

namespace StasisModule
{
	static class L10n
	{
#if GAME_SN
		public const string ids_seamothSM_Name = "Seamoth stasis module";
		public const string ids_seamothSM_Desc = "Generates a stationary stasis field around the Seamoth.";
#elif GAME_BZ
		public const string ids_seatruckSM_Name = "Seatruck stasis module";
		public const string ids_seatruckSM_Desc = "Generates a stationary stasis field around the Seatruck.";
#endif
		public const string ids_prawnSM_Name = "Prawn suit stasis module";
		public const string ids_prawnSM_Desc = "Generates a stationary stasis field around the Prawn suit.";
	}

	[CraftHelper.NoAutoPatch]
	class StasisModule: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new
		(
			new (TechType.AdvancedWiringKit, 2),
			new (TechType.Magnetite, 3)
		);

		protected override void initPrefabPool() => addPrefabToPool(TechType.ExosuitJetUpgradeModule);

		protected TechType _register(string name, string description) =>
			register(name, description, SpriteHelper.getSprite("stasismodule"));

		public override void patch()
		{
			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
#if GAME_SN
			setTechTypeForUnlock(TechType.StasisRifle);
#endif
		}
	}

#if GAME_SN
	class SeaMothStasisModule: StasisModule
	{
		public static new TechType TechType { get; private set; } = 0;

		public override void patch()
		{
			TechType = _register(L10n.ids_seamothSM_Name, L10n.ids_seamothSM_Desc);
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
			TechType = _register(L10n.ids_seatruckSM_Name, L10n.ids_seatruckSM_Desc);
			base.patch();

			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "SeaTruckUpgrade");
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/SeatruckUpgrades");
			setEquipmentType(EquipmentType.SeaTruckModule, QuickSlotType.Selectable);
			setTechTypeForUnlock(TechType.SeaTruck);
		}
	}
#endif
	class PrawnSuitStasisModule: StasisModule
	{
		public static new TechType TechType { get; private set; } = 0;

		public override void patch()
		{
			TechType = _register(L10n.ids_prawnSM_Name, L10n.ids_prawnSM_Desc);
			base.patch();

			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules");
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.Instant);
#if GAME_BZ
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/ExosuitUpgrades");
			setTechTypeForUnlock(TechType.Exosuit);
#endif
		}
	}
}