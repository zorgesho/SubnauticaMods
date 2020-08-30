using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class NutrientBlockCraftable: PoolCraftableObject
	{
		protected override TechData getTechData() => new TechData
		(
			new Ingredient(TechType.CuredPeeper, 1),
			new Ingredient(TechType.CuredReginald, 1),
			new Ingredient(TechType.PurpleVegetable, 2),
			new Ingredient(TechType.CreepvinePiece, 2)
		)	{ craftAmount = 1};

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

			addToGroup(TechGroup.Survival, TechCategory.CookedFood);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Survival");
			setTechTypeForUnlock(TechType.PurpleVegetable);
		}
	}
}