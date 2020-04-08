using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace GravTrapImproved
{
	class GravTrapMK2: CraftableObject
	{
		public class Tag: MonoBehaviour {} // just to distinguish between vanilla gravtrap and upgraded

		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.Titanium, 1)
		}};

		public override GameObject getGameObject()
		{
			var prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.Gravsphere));

			prefab.AddComponent<Tag>();

			return prefab;
		}

		public override void patch()
		{
			TechType = register("Grav trap MK2", "TODO", TechType.Gravsphere);

			addToGroup(TechGroup.Workbench, TechCategory.Workbench);
			addCraftingNodeTo(CraftTree.Type.Workbench, "ExosuitMenu");

			setItemSize(2, 2);
			setCraftingTime(5f);
			setEquipmentType(EquipmentType.Hand);

			unlockOnStart();
		}
	}
}