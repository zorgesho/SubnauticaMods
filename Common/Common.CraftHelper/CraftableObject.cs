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
		protected CraftableObject(string classID): base(classID, classID + "_Prefab") {}

		bool isUsingExactPrefab = false; // using result of getGameObject as prefab, without smlhelper additional stuff

		public abstract void patch();
		public abstract GameObject getGameObject();
		protected abstract TechData getTechData();

		public override GameObject GetGameObject()
		{
			if (isUsingExactPrefab)
			{
				$"CraftableObject.GetGameObject called, but isUsingExactPrefab is TRUE ({ClassID})".logError();
				Debug.logStack();
			}

			return isUsingExactPrefab? null: getGameObject();
		}

		void registerPrefabAndTechData()
		{
			PrefabHandler.RegisterPrefab(this);

			if (getTechData() is TechData techData)
				CraftDataHandler.SetTechData(TechType, techData);
		}

		protected void register(TechType techType) // for already existing techtypes
		{
			TechType = techType;
			registerPrefabAndTechData();
		}

		protected TechType register(string friendlyName, string description, Atlas.Sprite sprite = null)
		{
			if (sprite == null)
				sprite = SpriteManager._defaultSprite;

			TechType = TechTypeHandler.AddTechType(ClassID, friendlyName, description, sprite, false);
			
			registerPrefabAndTechData();

			return TechType;
		}

		protected void useExactPrefab()
		{																												$"Already using exact prefab! ({ClassID})".logDbgError(isUsingExactPrefab);
			isUsingExactPrefab = true;
			PrefabDatabasePatcher.addPrefab(this);
		}

		protected void addToGroup(TechGroup group, TechCategory category, TechType after = TechType.None)
		{
			CraftDataHandler.AddToGroup(group, category, TechType, after);

			if (group >= TechGroup.BasePieces && group <= TechGroup.Miscellaneous) // little hack
				CraftDataHandler.AddBuildable(TechType);
		}

		protected void setEquipmentType(EquipmentType equipmentType, QuickSlotType quickSlotType = QuickSlotType.None)
		{
			CraftDataHandler.SetEquipmentType(TechType, equipmentType);
			
			if (quickSlotType != QuickSlotType.None)
				CraftDataHandler.SetQuickSlotType(TechType, quickSlotType);
		}

		protected void setTechTypeForUnlock(TechType techType, string message = "NotificationBlueprintUnlocked")
		{
			KnownTechHandler.SetAnalysisTechEntry(techType, new TechType[1] { TechType }, message);
		}

		//protected void setAllTechTypesForUnlock(TechType t1, TechType t2)
		//{
		//	UnlockTechHelper.setTechTypesForUnlock(UnlockTechHelper.UnlockType.All, TechType, new TechType[] { t1, t2 });
		//}
		
		protected void addCraftingNode(CraftTree.Type craftTree, string craftPath) =>
			CraftTreeHandler.AddCraftingNode(craftTree, TechType, craftPath.Split('/'));

		protected void addCraftingNode(CraftTree.Type craftTree, string craftPath, TechType after) =>
			CraftNodesCustomOrder.addNode(craftTree, TechType, craftPath, after);

		protected void addCraftingNodeTo(ModCraftTreeLinkingNode modCraftTreeNode) =>
			modCraftTreeNode.AddCraftingNode(TechType);

		protected void setBackgroundType(CraftData.BackgroundType backgroundType) => CraftDataHandler.SetBackgroundType(TechType, backgroundType);

		protected void setItemSize(int width, int height) => CraftDataHandler.SetItemSize(TechType, width, height);

		protected void setCraftingTime(float time) => CraftDataHandler.SetCraftingTime(TechType, time);

		protected void unlockOnStart() => KnownTechHandler.UnlockOnStart(TechType);
	}
}