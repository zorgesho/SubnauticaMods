using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace MiscObjects
{
	class DiamondBlade: CraftableObject
	{
		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.Knife, 1),
			new Ingredient(TechType.Diamond, 2),
		}};

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(Resources.Load<GameObject>("WorldEntities/Tools/DiamondBlade"));
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