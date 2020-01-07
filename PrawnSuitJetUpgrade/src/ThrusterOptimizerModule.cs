using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace PrawnSuitJetUpgrade
{
	class PrawnThrustersOptimizer: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.AdvancedWiringKit, 1),
			new Ingredient(TechType.Sulphur, 3),
			new Ingredient(TechType.Aerogel, 2),
			new Ingredient(TechType.Polyaniline, 1),
		}};

		public override GameObject getGameObject() => Object.Instantiate(CraftData.GetPrefabForTechType(TechType.VehicleArmorPlating));

		public override void patch()
		{
			TechType = register("Prawn suit thrusters optimizer", "Thrusters work longer before need to recharge.");

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules");
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.Passive);

			setTechTypeForUnlock(TechType.BaseUpgradeConsole);
		}
	}
}