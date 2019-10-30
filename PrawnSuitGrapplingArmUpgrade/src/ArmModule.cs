using System.Collections.Generic;
using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace PrawnSuitGrapplingArmUpgrade
{
	abstract class CraftObject: ModPrefab
	{
		protected CraftObject(string classID): base(classID, classID + "Prefab") {}
		
		abstract protected TechData getTechData();
		abstract protected GameObject getGameObject();
		
		override public GameObject GetGameObject() => getGameObject();

		protected void addToCraftingNode(CraftTree.Type craftTree, string craftPath)
		{
			CraftTreeHandler.AddCraftingNode(craftTree, TechType, craftPath.Split('/'));
		}
		
		protected void setPDAGroup(TechGroup group, TechCategory category)
		{
			CraftDataHandler.AddToGroup(group, category, TechType);

			if (group >= TechGroup.BasePieces && group <= TechGroup.Miscellaneous) // little hack
				CraftDataHandler.AddBuildable(TechType);
		}
		
		protected void setEquipmentType(EquipmentType equipmentType, QuickSlotType quickSlotType = QuickSlotType.None)
		{
			CraftDataHandler.SetEquipmentType(TechType, equipmentType);
			
			if (quickSlotType != QuickSlotType.None)
				CraftDataHandler.SetQuickSlotType(TechType, quickSlotType);
		}
		
		TechType register(string friendlyName, string description, Atlas.Sprite sprite)
		{
			TechType = TechTypeHandler.AddTechType(ClassID, friendlyName, description, sprite);
			
			PrefabHandler.RegisterPrefab(this);
			CraftDataHandler.SetTechData(TechType, getTechData());

			return TechType;
		}
		
		protected TechType register(string friendlyName, string description, TechType spriteFrom) => register(friendlyName, description, SpriteManager.Get(spriteFrom));

		protected TechType register(string friendlyName, string description, string spriteFileName) => 0; // todo implement
	}
	
	
	class GrapplingArmUpgradeModule: CraftObject
	{
		new static public TechType TechType { get; private set; } = 0;

		GrapplingArmUpgradeModule(): base(nameof(GrapplingArmUpgradeModule)) {}

		static public void patch()
		{
			if (TechType == 0)
				new GrapplingArmUpgradeModule().patchMe();
		}

		override protected TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.Peeper, 1),
		}};

		override protected GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.ExosuitGrapplingArmModule));
			prefab.name = ClassID;

			return prefab;
		}

		void patchMe()
		{
			TechType = register("GrappoArm", "GrappoArm DESCR", TechType.ExosuitGrapplingArmModule);

			setPDAGroup(TechGroup.Workbench, TechCategory.Workbench);
			addToCraftingNode(CraftTree.Type.Workbench, "ExosuitMenu");
			setEquipmentType(EquipmentType.ExosuitArm, QuickSlotType.Selectable);
		}
	}
}
