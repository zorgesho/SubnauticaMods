using Common.Crafting;

namespace StasisTorpedo
{
	partial class StasisTorpedo: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new // TODO
		(
			new (TechType.Titanium),
			new (TechType.Gold)
		);

		protected override void initPrefabPool() => addPrefabToPool(TechType.GasTorpedo);

		public override void patch()
		{
			TechType = register(); // TODO

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades, TechType.GasTorpedo);
#if GAME_SN
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "Torpedoes");
#elif GAME_BZ
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules", TechType.GasTorpedo);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Upgrades/ExosuitUpgrades", TechType.GasTorpedo);
#endif
			unlockOnStart(); // TODO
		}
	}
}