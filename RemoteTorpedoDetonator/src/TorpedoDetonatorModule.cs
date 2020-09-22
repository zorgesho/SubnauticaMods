using SMLHelper.V2.Crafting;
using Common.Crafting;

namespace RemoteTorpedoDetonator
{
	class TorpedoDetonatorModule: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData
		(
			new Ingredient(TechType.AdvancedWiringKit, 1),
			new Ingredient(TechType.Magnetite, 1)
		)	{ craftAmount = 1};

		protected override void initPrefabPool() => addPrefabToPool(TechType.VehicleArmorPlating);

		public override void patch()
		{
			TechType = register(L10n.ids_detonatorName, L10n.ids_detonatorDesc);

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "CommonModules");
			setEquipmentType(EquipmentType.VehicleModule, QuickSlotType.Instant);

			setTechTypeForUnlock(TechType.GasTorpedo);
		}
	}
}