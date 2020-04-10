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
			new Ingredient(TechType.Gravsphere, 1),
			new Ingredient(TechType.Titanium, 2),
			new Ingredient(TechType.PowerCell, 1),
			new Ingredient(TechType.AdvancedWiringKit, 2)
		}};

		public override GameObject getGameObject()
		{
			var prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.Gravsphere));

			prefab.AddComponent<Tag>();

			return prefab;
		}

		public override void patch()
		{
			TechType = register(L10n.ids_GravTrapMK2, L10n.ids_GravTrapMK2Description, TechType.Gravsphere);

			addToGroup(TechGroup.Workbench, TechCategory.Workbench);

			CraftNodesCustomOrder.addNode(CraftTree.Type.Workbench, "GravTrap", L10n.ids_GravTrapMenu, "", "FinsMenu", TechType.Gravsphere);
			addCraftingNodeTo(CraftTree.Type.Workbench, "GravTrap", TechType.None);

			setItemSize(2, 2);
			setCraftingTime(5f);
			setEquipmentType(EquipmentType.Hand);

			if (Main.config.mk2FragmentCountToUnlock > 0)
				setFragmentToUnlock(TechType.GravSphereFragment, Main.config.mk2FragmentCountToUnlock, 5f);
			else
				setTechTypeForUnlock(TechType.Gravsphere);
		}
	}
}