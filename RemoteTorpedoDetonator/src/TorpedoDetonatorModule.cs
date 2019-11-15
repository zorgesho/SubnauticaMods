using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace RemoteTorpedoDetonator
{
	class TorpedoDetonatorModule: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.AdvancedWiringKit, 1),
			new Ingredient(TechType.Magnetite, 1),
		}};

		protected override GameObject getGameObject() => Object.Instantiate(CraftData.GetPrefabForTechType(TechType.VehicleArmorPlating));

		public override void patch()
		{
			TechType = register("Remote torpedo detonator",
								"Allows detonate launched torpedoes remotely. Seamoth/Prawn compatible.",
								AssetsHelper.loadSprite(ClassID));

			setPDAGroup(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
			addToCraftingNode(CraftTree.Type.SeamothUpgrades, "CommonModules");
			setEquipmentType(EquipmentType.VehicleModule, QuickSlotType.Instant);

			setTechTypeForUnlock(TechType.GasTorpedo);
		}
	}
}