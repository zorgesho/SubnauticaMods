using UnityEngine;
using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace PrawnSuitJetUpgrade
{
	class PrawnThrustersOptimizer: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData
		(
			new Ingredient(TechType.AdvancedWiringKit, 1),
			new Ingredient(TechType.Sulphur, 3),
			new Ingredient(TechType.Aerogel, 2),
			new Ingredient(TechType.Polyaniline, 1)
		)	{ craftAmount = 1 };

		public override GameObject getGameObject() => CraftHelper.Utils.prefabCopy(TechType.VehicleArmorPlating);

		public override void patch()
		{
			TechType = register(L10n.ids_optimizerName, L10n.ids_optimizerDesc);

			addToGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			addCraftingNodeTo(CraftTree.Type.SeamothUpgrades, "ExosuitModules");
			setEquipmentType(EquipmentType.ExosuitModule, QuickSlotType.Passive);

			setTechTypeForUnlock(TechType.BaseUpgradeConsole);
		}
	}
}