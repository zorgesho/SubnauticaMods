using Common.Crafting;

namespace PrawnSuitJetUpgrade
{
	class PrawnThrustersOptimizer: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new
		(
			new TechInfo.Ing(TechType.AdvancedWiringKit),
			new TechInfo.Ing(TechType.Sulphur, 3),
			new TechInfo.Ing(TechType.Aerogel, 2),
			new TechInfo.Ing(TechType.Polyaniline)
		);

		protected override void initPrefabPool() => addPrefabToPool(TechType.ExosuitJetUpgradeModule);

		public override void patch()
		{
			TechType = register(L10n.ids_optimizerName, L10n.ids_optimizerDesc);

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.ExosuitJetUpgradeModule);
#if GAME_SN
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules", TechType.ExosuitJetUpgradeModule);
#elif GAME_BZ
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/ExosuitUpgrades", TechType.ExosuitJetUpgradeModule);
#endif
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.Passive);
			setTechTypeForUnlock(TechType.ExosuitJetUpgradeModule);
		}
	}
}