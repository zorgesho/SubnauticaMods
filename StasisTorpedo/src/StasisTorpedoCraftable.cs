using Common.Crafting;

namespace StasisTorpedo
{
	partial class StasisTorpedo: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new
		(
			new (TechType.ComputerChip),
			new (TechType.Magnetite, 2),
			new (TechType.Titanium)
		);

		protected override void initPrefabPool() => addPrefabToPool(TechType.GasTorpedo);

		public override void patch()
		{
			TechType = register("Stasis torpedo", "Generates a localized stasis field. Load this to a vehicle torpedo bay.");

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.GasTorpedo);
#if GAME_SN
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "Torpedoes");
			setTechTypeForUnlock(TechType.StasisRifle);
#elif GAME_BZ
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules", TechType.GasTorpedo);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/ExosuitUpgrades", TechType.GasTorpedo);
			setTechTypeForUnlock(TechType.GasTorpedo);
#endif
		}
	}
}