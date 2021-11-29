using UnityEngine;
using SMLHelper.V2.Assets;

using Common.Crafting;

namespace StasisTorpedo
{
	class StasisTorpedo: PoolCraftableObject
	{
		static GameObject stasisSpherePrefab;

		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new // TODO
		(
			new (TechType.Titanium),
			new (TechType.Gold)
		);

		protected override void initPrefabPool()
		{
			addPrefabToPool(TechType.GasTorpedo);
			addPrefabToPool(TechType.StasisRifle);
		}

		protected override GameObject getGameObject(GameObject[] prefabs)
		{
			stasisSpherePrefab ??= ModPrefabCache.AddPrefabCopy(prefabs[1].GetComponent<StasisRifle>().effectSpherePrefab, false);
			return prefabs[0];
		}

		public override void patch()
		{
			TechType = register(); // TODO

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades); // TODO check for BZ
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "Torpedoes");
			unlockOnStart(); // TODO
		}
	}
}