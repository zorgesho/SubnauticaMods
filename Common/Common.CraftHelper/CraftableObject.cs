using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

#if GAME_SN
	using Sprite = Atlas.Sprite;
	using RecipeData = SMLHelper.V2.Crafting.TechData;
#elif GAME_BZ
	using Sprite = UnityEngine.Sprite;
	using RecipeData = SMLHelper.V2.Crafting.RecipeData;
#endif

namespace Common.Crafting
{
	using Reflection;

	abstract class CraftableObject: ModPrefab
	{
		protected CraftableObject(): this(ReflectionHelper.getCallingDerivedType().Name) {}
		protected CraftableObject(string classID): base(classID, classID + "_Prefab") {}

		bool isUsingExactPrefab = false; // using result of getGameObject as prefab, without smlhelper additional stuff

		public abstract void patch();

		public virtual GameObject getGameObject() => null;
		public virtual IEnumerator getGameObjectAsync(IOut<GameObject> result) => null;

		protected abstract RecipeData getTechData();

		public sealed override GameObject GetGameObject()
		{
			Debug.assert(!isUsingExactPrefab);
			return isUsingExactPrefab? null: getGameObject();
		}
		public sealed override IEnumerator GetGameObjectAsync(IOut<GameObject> result)
		{
			Debug.assert(!isUsingExactPrefab);
			return isUsingExactPrefab? null: getGameObjectAsync(result);
		}

		void registerPrefabAndTechData()
		{
			PrefabHandler.RegisterPrefab(this);

			if (getTechData() is RecipeData recipeData)
				CraftDataHandler.SetTechData(TechType, recipeData);
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

		protected TechType register() => // just for convenience during development
			register(ClassID, ClassID);

		protected TechType register(string name, string description) => // using external sprite
			register(name, description, SpriteHelper.getSprite(ClassID));

		protected TechType register(string name, string description, TechType spriteTechType) => // using sprite for another techtype
			register(name, description, SpriteHelper.getSprite(spriteTechType));

		protected TechType register(string name, string description, Sprite sprite)
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


	// class that simplify work with sync/async prefabs
	abstract class PoolCraftableObject: CraftableObject
	{
		class PrefabInfo
		{
			public bool copy; // create prefab copy

			// get prefab either by techType or filepath
			public TechType techType;
			public string filepath;
		}

		List<PrefabInfo> poolPrefabInfo;

		protected int addPrefabToPool(TechType techType, bool copy = true)
		{
			Debug.assert(poolPrefabInfo != null);

			poolPrefabInfo.Add(new PrefabInfo() { techType = techType, copy = copy });
			return poolPrefabInfo.Count;
		}

		protected int addPrefabToPool(string filepath, bool copy = true)
		{
			Debug.assert(poolPrefabInfo != null);

			poolPrefabInfo.Add(new PrefabInfo() { filepath = filepath, copy = copy });
			return poolPrefabInfo.Count;
		}

		GameObject preparePrefab(PrefabInfo info)
		{
#if BRANCH_EXP
			return null;
#elif BRANCH_STABLE
			Debug.assert(info.techType != default || info.filepath != null);

			if (info.techType != default)
				return info.copy? PrefabUtils.getPrefabCopy(info.techType): PrefabUtils.getPrefab(info.techType);
			else
				return info.copy? PrefabUtils.getPrefabCopy(info.filepath): PrefabUtils.getPrefab(info.filepath);
#endif
		}

		IEnumerator preparePrefabAsync(PrefabInfo info, IOut<GameObject> result)
		{
			Debug.assert(info.techType != default || info.filepath != null);

			CoroutineTask<GameObject> task;

			if (info.techType != default)
				task = info.copy? PrefabUtils.getPrefabCopyAsync(info.techType): PrefabUtils.getPrefabAsync(info.techType);
			else
				task = info.copy? PrefabUtils.getPrefabCopyAsync($"{info.filepath}.prefab"): PrefabUtils.getPrefabAsync($"{info.filepath}.prefab");

			yield return task;
			result.Set(task.GetResult());
		}

		void preparePool()
		{
			if (poolPrefabInfo != null)
				return;

			poolPrefabInfo = new List<PrefabInfo>();
			initPrefabPool();
		}

		GameObject _processPrefabs(GameObject[] prefabs)
		{
			if (prefabs.Length == 0 || prefabs.Any(prefab => prefab == null))
				return null;

			return prefabs.Length == 1? getGameObject(prefabs[0]): getGameObject(prefabs);
		}

		public sealed override GameObject getGameObject()
		{
			preparePool();

			var prefabs = poolPrefabInfo.Select(info => preparePrefab(info)).ToArray();
			return _processPrefabs(prefabs);
		}

		public sealed override IEnumerator getGameObjectAsync(IOut<GameObject> result)
		{
			preparePool();

			var prefabs = new GameObject[poolPrefabInfo.Count];

			for (int i = 0; i < poolPrefabInfo.Count; i++)
			{
				var taskResult = new TaskResult<GameObject>();
				yield return preparePrefabAsync(poolPrefabInfo[i], taskResult);

				prefabs[i] = taskResult.Get();
			}

			result.Set(_processPrefabs(prefabs));
		}

		protected abstract void initPrefabPool();

		protected virtual GameObject getGameObject(GameObject prefab) => prefab;
		protected virtual GameObject getGameObject(GameObject[] prefabs) => null;
	}
}