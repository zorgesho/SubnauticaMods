using UnityEngine;
using Common.Crafting;

namespace StasisModule
{
	[CraftHelper.NoAutoPatch]
	partial class StasisModule: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new // TODO
		(
			new (TechType.Titanium),
			new (TechType.Gold)
		);

		protected override void initPrefabPool()
		{
			addPrefabToPool(TechType.VehicleArmorPlating);
#if GAME_BZ
#pragma warning disable CS0612
#endif
			addPrefabToPool(TechType.StasisRifle);
#if GAME_BZ
#pragma warning restore CS0612
#endif
		}

		protected override GameObject getGameObject(GameObject[] prefabs)
		{
			StasisExplosion.initPrefab(prefabs[1]);
			return prefabs[0];
		}

		public override void patch()
		{
			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			unlockOnStart(); // TODO
		}
	}

	class SeamothStasisModule: StasisModule
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

	class PrawnSuitStasisModule: StasisModule
	{
		public static new TechType TechType { get; private set; } = 0;

		public override void patch()
		{
			TechType = register(); // TODO
			base.patch();

			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules");
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.Instant);
		}
	}
}