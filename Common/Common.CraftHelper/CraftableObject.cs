﻿using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace Common.Crafting
{
	[CraftHelper.NoAutoPatch]
	abstract class CraftableObject: ModPrefab
	{
		protected CraftableObject(): this(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().ReflectedType.Name) {}
		protected CraftableObject(string classID): base(classID, classID + "Prefab") {}
		
		public abstract void patch();
		protected abstract TechData getTechData();
		protected abstract GameObject getGameObject();

		public override GameObject GetGameObject()
		{
			GameObject prefab = getGameObject();
			prefab.name = ClassID;
			
			return prefab;
		}


		public TechType register(string friendlyName, string description, Atlas.Sprite sprite = null)
		{
			if (sprite == null)
				sprite = SpriteManager._defaultSprite;
			
			TechType = TechTypeHandler.AddTechType(ClassID, friendlyName, description, sprite, false);
			
			PrefabHandler.RegisterPrefab(this);
			CraftDataHandler.SetTechData(TechType, getTechData());

			return TechType;
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


		protected void setTechTypeForUnlock(TechType techType, string message = null)
		{
			if (message == null)
				message = "NotificationBlueprintUnlocked";
			
			KnownTechHandler.SetAnalysisTechEntry(techType, new TechType[1] { TechType }, message);
		}

		//protected void setAllTechTypesForUnlock(TechType t1, TechType t2)
		//{
		//	UnlockTechHelper.setTechTypesForUnlock(UnlockTechHelper.UnlockType.All, TechType, new TechType[] { t1, t2 });
		//}
		
		protected void addToCraftingNode(CraftTree.Type craftTree, string craftPath) => CraftTreeHandler.AddCraftingNode(craftTree, TechType, craftPath.Split('/'));

		protected void setBackgroundType(CraftData.BackgroundType backgroundType) => CraftDataHandler.SetBackgroundType(TechType, backgroundType);
		
		protected void setItemSize(int width, int height) => CraftDataHandler.SetItemSize(TechType, width, height);
		
		protected void setCraftingTime(float time) => CraftDataHandler.SetCraftingTime(TechType, time);
		
		protected void unlockOnStart() => KnownTechHandler.UnlockOnStart(TechType);
	}
}