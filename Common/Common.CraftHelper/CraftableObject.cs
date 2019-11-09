using UnityEngine;

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
		
		protected abstract TechData getTechData();
		protected abstract GameObject getGameObject();
		public abstract void patch();

		public override GameObject GetGameObject()
		{
			GameObject prefab = getGameObject();
			prefab.name = ClassID;
			
			return prefab;
		}


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

		public TechType register(string friendlyName, string description, Atlas.Sprite sprite = null)
		{
			if (sprite == null)
				sprite = SpriteManager._defaultSprite;
			
			TechType = TechTypeHandler.AddTechType(ClassID, friendlyName, description, sprite, false);
			
			PrefabHandler.RegisterPrefab(this);
			CraftDataHandler.SetTechData(TechType, getTechData());

			return TechType;
		}
	}
}