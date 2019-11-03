using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace Common.CraftHelper
{
	abstract class CraftableObject: ModPrefab
	{
		protected CraftableObject(string classID): base(classID, classID + "Prefab") {}
		
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


		protected void setBackgroundType(CraftData.BackgroundType backgroundType)
		{
			CraftDataHandler.SetBackgroundType(TechType, backgroundType);
		}
		
		protected void unlockOnStart()
		{
			KnownTechHandler.UnlockOnStart(TechType);
		}

		//protected void setAllTechTypesForUnlock(TechType t1, TechType t2)
		//{
		//	UnlockTechHelper.setTechTypesForUnlock(UnlockTechHelper.UnlockType.All, TechType, new TechType[] { t1, t2 });
		//}

		TechType register(string friendlyName, string description, Atlas.Sprite sprite)
		{
			TechType = TechTypeHandler.AddTechType(ClassID, friendlyName, description, sprite, false);
			
			PrefabHandler.RegisterPrefab(this);
			CraftDataHandler.SetTechData(TechType, getTechData());

			return TechType;
		}
		
		protected TechType register(string friendlyName, string description, TechType spriteFrom) => register(friendlyName, description, SpriteManager.Get(spriteFrom));

		protected TechType register(string friendlyName, string description, string spriteFileName) => 0; // todo implement
	}
}
