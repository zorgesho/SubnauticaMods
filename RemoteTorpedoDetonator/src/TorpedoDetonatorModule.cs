using Common.Crafting;

namespace RemoteTorpedoDetonator
{
	class TorpedoDetonatorModule: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new
		(
			new (TechType.AdvancedWiringKit),
			new (TechType.Magnetite)
		);

		protected override void initPrefabPool() => addPrefabToPool(TechType.ExosuitJetUpgradeModule);

		public override void patch()
		{
			TechType = register(L10n.ids_detonatorName, L10n.ids_detonatorDesc);
#if GAME_SN
			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.VehicleStorageModule);
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "CommonModules", TechType.VehicleStorageModule);
#elif GAME_BZ
			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.ExosuitThermalReactorModule);
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules", TechType.ExosuitThermalReactorModule);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/ExosuitUpgrades", TechType.ExosuitThermalReactorModule);
#endif
			setEquipmentType(EquipmentType.VehicleModule, QuickSlotType.Instant);
			setTechTypeForUnlock(TechType.GasTorpedo);
		}
	}
}