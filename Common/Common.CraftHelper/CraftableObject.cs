using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

#if GAME_SN
	using Sprite = Atlas.Sprite;
#elif GAME_BZ
	using Sprite = UnityEngine.Sprite;
#endif

namespace Common.Crafting
{
	using Harmony;
	using Reflection;

	// blocks SMLHelper from processing prefab (so we can use exact prefab)
	static class PrefabProcessingBlocker
	{
		public static bool block = false;

		static bool inited = false;
		public static void init()
		{
			if (!inited && (inited = true))
				HarmonyHelper.patch();
		}

		[HarmonyPrefix, HarmonyHelper.Patch("SMLHelper.V2.Assets.ModPrefab, SMLHelper", "ProcessPrefab")]
		static bool blockPrefabProcessing() => !block || (block = false);
	}

	abstract class CraftableObject: ModPrefab
	{
		protected CraftableObject(): this(ReflectionHelper.getCallingDerivedType().Name) {}
		protected CraftableObject(string classID): base(classID, classID + "_Prefab") {}

		bool isUsingExactPrefab = false; // using result of getGameObject as prefab, without smlhelper additional stuff

		public abstract void patch();

		public virtual GameObject getGameObject() => null;
		public virtual IEnumerator getGameObjectAsync(IOut<GameObject> result) => null;

		protected abstract TechInfo getTechInfo();

		public sealed override GameObject GetGameObject()
		{
			PrefabProcessingBlocker.block = isUsingExactPrefab;
			return getGameObject();
		}
		public sealed override IEnumerator GetGameObjectAsync(IOut<GameObject> result)
		{
			PrefabProcessingBlocker.block = isUsingExactPrefab;
			return getGameObjectAsync(result);
		}

		void registerPrefabAndTechInfo()
		{
			PrefabHandler.RegisterPrefab(this);

			if (getTechInfo() is TechInfo techInfo)
				CraftDataHandler.SetTechData(TechType, techInfo);
		}

		protected void useExactPrefab()
		{
			isUsingExactPrefab = true;
			PrefabProcessingBlocker.init();
		}


		protected void register(TechType techType) // for already existing techtypes
		{
			TechType = techType;
			registerPrefabAndTechInfo();
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

			registerPrefabAndTechInfo();

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
			public bool copy { get; init; } // create prefab copy
			public string filename { get; init; }

			public PrefabInfo(string filename) => this.filename = filename;
			public PrefabInfo(TechType techType): this(PrefabUtils.getPrefabFilename(techType)) {}
		}

		List<PrefabInfo> poolPrefabInfo;

		protected PoolCraftableObject(): base() {}
		protected PoolCraftableObject(string classID): base(classID) {}

		int addPrefabToPool(PrefabInfo info)
		{
			Debug.assert(poolPrefabInfo != null);

			poolPrefabInfo.Add(info);
			return poolPrefabInfo.Count;
		}

		protected int addPrefabToPool(string filepath, bool copy = true) => addPrefabToPool(new (filepath) { copy = copy });
		protected int addPrefabToPool(TechType techType, bool copy = true) => addPrefabToPool(new (techType) { copy = copy });

		GameObject preparePrefab(PrefabInfo info)
		{
#if BRANCH_EXP
			return null;
#elif BRANCH_STABLE
			Debug.assert(info.filename != null);
			return info.copy? PrefabUtils.getPrefabCopy(info.filename): PrefabUtils.getPrefab(info.filename);
#endif
		}

		IEnumerator preparePrefabAsync(PrefabInfo info, IOut<GameObject> result)
		{
			Debug.assert(info.filename != null);

			var filename = Paths.ensureExtension(info.filename, "prefab");
			var task = info.copy? PrefabUtils.getPrefabCopyAsync(filename): PrefabUtils.getPrefabAsync(filename);
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

			var prefabs = poolPrefabInfo.Select(preparePrefab).ToArray();
			return _processPrefabs(prefabs);
		}

		public sealed override IEnumerator getGameObjectAsync(IOut<GameObject> result)
		{
			preparePool();

			var prefabs = new GameObject[poolPrefabInfo.Count];

			for (int i = 0; i < poolPrefabInfo.Count; i++)
			{
				TaskResult<GameObject> taskResult = new();
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