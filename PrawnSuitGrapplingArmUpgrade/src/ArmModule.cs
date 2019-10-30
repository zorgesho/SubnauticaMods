using System.Collections.Generic;
using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace PrawnSuitGrapplingArmUpgrade
{
	internal class GrapArm: Craftable
	{
		internal static Craftable PatchMe()
		{
			Singleton.Patch();
			return Singleton;
		}

		internal static TechType TechTypeID => Singleton.TechType;

		private static GrapArm Singleton { get; } = new GrapArm();

		public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;
		public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
		public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
		public override string[] StepsToFabricatorTab => new[] { "ExosuitModules" };
		public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;

		public override string AssetsFolder => "PrawnSuitJetUpgrade/Assets";

		protected GrapArm(): base(classId: "GrapArm", friendlyName: "GrapArm", description: "GrapArm")
		{
			base.OnFinishedPatching += PostPatch;
		}

		private void PostPatch()
		{
			CraftDataHandler.SetEquipmentType(this.TechType, EquipmentType.ExosuitArm);
			CraftDataHandler.SetQuickSlotType(this.TechType, QuickSlotType.Selectable);
		}

		protected override TechData GetBlueprintRecipe() => new TechData()
		{
			craftAmount = 1,
			Ingredients = new List<Ingredient>(new Ingredient[4]
							 {
								 new Ingredient(TechType.AdvancedWiringKit, 1),
								 new Ingredient(TechType.Sulphur, 3),
								 new Ingredient(TechType.Aerogel, 2),
								 new Ingredient(TechType.Polyaniline, 1),
							 })
		};

		public static GameObject GetGameObjectStatic()
		{
			var prefab =  GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.ExosuitGrapplingArmModule));

			return prefab;
		}
		
		public override GameObject GetGameObject()
		{
			return GetGameObjectStatic();
		}
	}
}
