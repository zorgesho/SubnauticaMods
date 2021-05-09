#if GAME_SN
using UnityEngine;
using Common.Crafting;

namespace MiscObjects
{
	class DiamondBlade: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new
		(
			new (TechType.Knife),
			new (TechType.Diamond, 2)
		);

		protected override void initPrefabPool() => addPrefabToPool("WorldEntities/Tools/DiamondBlade");

		protected override GameObject getGameObject(GameObject prefab)
		{
			prefab.GetComponent<Knife>().bleederDamage = 30f;
			return prefab;
		}

		public override void patch()
		{
			register(TechType.DiamondBlade);

			addToGroup(TechGroup.Workbench, TechCategory.Workbench, TechType.HeatBlade);
			addCraftingNodeTo(CraftTree.Type.Workbench, "KnifeMenu");
			setTechTypeForUnlock(TechType.Diamond);
		}
	}
}
#endif