using UnityEngine;
using Common.Crafting;

namespace StasisModule
{
	partial class StasisModule: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

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
			TechType = register(); // TODO

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "CommonModules");

			// TODO make it work for both seamoth and prawn
			//setEquipmentType(EquipmentType.VehicleModule, QuickSlotType.Selectable); // for seamoth
			setEquipmentType(EquipmentType.VehicleModule, QuickSlotType.Instant); // for prawn

			unlockOnStart(); // TODO
		}
	}
}