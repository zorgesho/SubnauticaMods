using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace PrawnSuitJetUpgrade
{
	class PrawnThrustersOptimizer: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		public static void patch()
		{
			if (TechType == 0)
				new PrawnThrustersOptimizer().patchMe();
		}

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.AdvancedWiringKit, 1),
			new Ingredient(TechType.Sulphur, 3),
			new Ingredient(TechType.Aerogel, 2),
			new Ingredient(TechType.Polyaniline, 1),
		}};

		protected override GameObject getGameObject() => Object.Instantiate(CraftData.GetPrefabForTechType(TechType.VehicleArmorPlating));

		void patchMe()
		{
			TechType = register("Prawn suit thrusters optimizer", "Thrusters work longer before need to recharge.", AssetsHelper.loadSprite(ClassID));

			setPDAGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			addToCraftingNode(CraftTree.Type.SeamothUpgrades, "ExosuitModules");
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.Passive);
				
//				public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;

			unlockOnStart();
		}
	}
}