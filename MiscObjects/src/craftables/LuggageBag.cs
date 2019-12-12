using System.Collections.Generic;
using UnityEngine;
using SMLHelper.V2.Crafting;
using Common.Crafting;

namespace MiscObjects
{
	class LuggageBag: CraftableObject
	{
		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.FiberMesh, 2),
			new Ingredient(TechType.Silicone, 1),
		}};

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(Resources.Load<GameObject>("WorldEntities/Doodads/Debris/Wrecks/Decoration/docking_luggage_01_bag4"));

			var fabricating = prefab.FindChild("model").AddComponent<VFXFabricating>();
			fabricating.localMinY = -0.2f;
			fabricating.localMaxY = 0.7f;
			fabricating.posOffset = new Vector3(0f, 0f, 0.04f);
			fabricating.scaleFactor = 0.8f;

			return prefab;
		}

		public override void patch()
		{
			register(TechType.LuggageBag);

			addToGroup(TechGroup.Personal, TechCategory.Equipment);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Personal/Equipment");
			setTechTypeForUnlock(TechType.LuggageBag);
		}
	}
}