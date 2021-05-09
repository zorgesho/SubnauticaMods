using UnityEngine;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class NutrientBlockCraftable: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new
		(
#if GAME_SN
			new (TechType.CuredPeeper),
			new (TechType.CuredReginald),
#elif GAME_BZ
			new (TechType.CuredArcticPeeper, 2), // TODO: ingredients?
			new (TechType.SmallMaroonPlantFruit),
#endif
			new (TechType.PurpleVegetable, 2),
			new (TechType.CreepvinePiece, 2)
		);

		protected override void initPrefabPool() => addPrefabToPool(TechType.NutrientBlock);

		protected override GameObject getGameObject(GameObject prefab)
		{
			var food = prefab.GetComponent<Eatable>();
			food.foodValue = 60;
			food.waterValue = -5;

			prefab.getChild("Nutrient_block").AddComponent<VFXFabricating>();
			PrefabUtils.initVFXFab(prefab, eulerOffset:new Vector3(-90f, 90f, 0f), localMinY: -0.05f, localMaxY: 0.12f);

			MeshRenderer meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
			meshRenderer.material.color = new Color(1, 1, 0, 1);

			return prefab;
		}

		public override void patch()
		{
			register("Craftable nutrient block", "Nutrient block cooked from local ingridients. Less nutritious and more salty than original.");
#if GAME_SN
			addToGroup(TechGroup.Survival, TechCategory.CookedFood);
#elif GAME_BZ
			addToGroup(TechGroup.Survival, TechCategory.FoodAndDrinks);
#endif
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Survival" + (Mod.Consts.isGameSN? "": "/CookedFood"));
			setTechTypeForUnlock(TechType.PurpleVegetable);
		}
	}
}