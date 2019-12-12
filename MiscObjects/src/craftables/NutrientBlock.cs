using System.Collections.Generic;
using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class NutrientBlockCraftable: CraftableObject
	{
		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.CuredPeeper, 1),
			new Ingredient(TechType.CuredReginald, 1),
			new Ingredient(TechType.PurpleVegetable, 2),
			new Ingredient(TechType.CreepvinePiece, 2),
		}};

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.NutrientBlock));
			
			var food = prefab.GetComponent<Eatable>();
			food.foodValue = 60;
			food.waterValue = -5;

			var fabricating = prefab.FindChild("Nutrient_block").AddComponent<VFXFabricating>();
			fabricating.localMinY = -0.05f;
			fabricating.localMaxY = 0.12f;
			fabricating.eulerOffset = new Vector3(-90f, 90f, 0f);

			MeshRenderer meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
			meshRenderer.material.color = new Color(1, 1, 0, 1);
			
			return prefab;
		}

		public override void patch()
		{
			register("Craftable nutrient block",
					 "Nutrient block cooked from local ingridients. Less nutritious and more salty than original.",
					 AssetsHelper.loadSprite(ClassID));

			addToGroup(TechGroup.Survival, TechCategory.CookedFood);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Survival");
			setTechTypeForUnlock(TechType.PurpleVegetable);
		}
	}
}