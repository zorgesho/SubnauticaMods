using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace Common.Crafting
{
	abstract class CraftableObject: ModPrefab
	{
		protected CraftableObject(): this(ReflectionHelper.getCallingType().Name) {}
		protected CraftableObject(string classID): base(classID, classID + "_Prefab") {}

		bool isUsingExactPrefab = false; // using result of getGameObject as prefab, without smlhelper additional stuff

		public abstract void patch();
		public abstract GameObject getGameObject();
		protected abstract TechData getTechData();

		public override GameObject GetGameObject()
		{
			if (isUsingExactPrefab && $"CraftableObject.GetGameObject called, but isUsingExactPrefab is TRUE ({ClassID})".logError())
				Debug.logStack();

			return isUsingExactPrefab? null: getGameObject();
		}

		void registerPrefabAndTechData()
		{
			PrefabHandler.RegisterPrefab(this);

			if (getTechData() is TechData techData)
				CraftDataHandler.SetTechData(TechType, techData);
		}

		protected void useExactPrefab()
		{																												$"Already using exact prefab! ({ClassID})".logDbgError(isUsingExactPrefab);
			isUsingExactPrefab = true;
			PrefabDatabasePatcher.addPrefab(this);
		}


		protected void register(TechType techType) // for already existing techtypes
		{
			TechType = techType;
			registerPrefabAndTechData();
		}

		protected TechType register() =>  // just for convenience during development
			register(ClassID, ClassID);

		protected TechType register(string name, string description) => // using external sprite
			register(name, description, SpriteHelper.getSprite(ClassID));

		protected TechType register(string name, string description, TechType spriteTechType) => // using sprite for another techtype
			register(name, description, SpriteHelper.getSprite(spriteTechType));

		protected TechType register(string name, string description, Atlas.Sprite sprite)
		{
			TechType = TechTypeHandler.AddTechType(ClassID, name, description, sprite, false);

			registerPrefabAndTechData();

			return TechType;
		}

		protected void useTextFrom(TechType nameFrom = TechType.None, TechType descriptionFrom = TechType.None)
		{
			if (nameFrom != TechType.None)
				LanguageHelper.substituteString(ClassID, nameFrom.AsString());

			if (descriptionFrom != TechType.None)
				LanguageHelper.substituteString("Tooltip_" + ClassID, "Tooltip_" + descriptionFrom.AsString());
		}


		protected void unlockOnStart() => KnownTechHandler.UnlockOnStart(TechType);

		protected void setTechTypeForUnlock(TechType techType) =>
			KnownTechHandler.SetAnalysisTechEntry(techType, new TechType[1] { TechType });

		// for using already existing fragments (will be used for this tech if fragment own tech is unlocked)
		protected void setFragmentToUnlock(TechType fragTechType, int fragCount, float scanTime = 1f)
		{
			string fragTechID = ClassID + "_Fragment";

			TechType substFragTechType = TechTypeHandler.AddTechType(fragTechID, "", "");
			LanguageHelper.substituteString(fragTechID, fragTechType.AsString()); // use name from original fragment

			UnlockTechHelper.setFragmentTypeToUnlock(TechType, fragTechType, substFragTechType, fragCount, scanTime);
		}


		protected void addToGroup(TechGroup group, TechCategory category, TechType after = TechType.None)
		{
			CraftDataHandler.AddToGroup(group, category, TechType, after);

			if (group >= TechGroup.BasePieces && group <= TechGroup.Miscellaneous) // little hack
				CraftDataHandler.AddBuildable(TechType);
		}

		protected void addCraftingNodeTo(CraftTree.Type craftTree, string craftPath) =>
			CraftTreeHandler.AddCraftingNode(craftTree, TechType, craftPath.Split('/'));

		protected void addCraftingNodeTo(CraftTree.Type craftTree, string craftPath, TechType after) =>
			CraftNodesCustomOrder.addNode(craftTree, TechType, craftPath, after);

		protected void addCraftingNodeTo(ModCraftTreeLinkingNode modCraftTreeNode) =>
			modCraftTreeNode.AddCraftingNode(TechType);


		protected void setEquipmentType(EquipmentType equipmentType, QuickSlotType quickSlotType = QuickSlotType.None)
		{
			CraftDataHandler.SetEquipmentType(TechType, equipmentType);

			if (quickSlotType != QuickSlotType.None)
				CraftDataHandler.SetQuickSlotType(TechType, quickSlotType);
		}

		protected void setBackgroundType(CraftData.BackgroundType backgroundType) => CraftDataHandler.SetBackgroundType(TechType, backgroundType);

		protected void setItemSize(int width, int height) => CraftDataHandler.SetItemSize(TechType, width, height);

		protected void setCraftingTime(float time) => CraftDataHandler.SetCraftingTime(TechType, time);
	}
}