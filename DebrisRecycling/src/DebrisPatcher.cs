using System.Collections.Generic;
using UnityEngine;
using Common;

namespace DebrisRecycling
{
	class DebrisDeconstructable: MonoBehaviour, IConstructable
	{
		public bool CanDeconstruct(out string reason)
		{
			reason = L10n.str("ids_tryMoveObject");

			if (Main.config.deconstructValidStaticObjects)
				return true;

			return gameObject.getComponentInHierarchy<Rigidbody>(true, false)?.isKinematic == false;
		}

		public void OnConstructedChanged(bool constructed) => DebrisTracker.untrack(gameObject);
	}


	static class DebrisPatcher
	{
		static PrefabsConfig prefabsConfig;
		static Dictionary<string, int> validPrefabs;

		public static bool isValidObject(GameObject go) => isValidPrefab(go?.GetComponent<PrefabIdentifier>()?.ClassId);
		public static bool isValidPrefab(string prefabID) => prefabID != null? validPrefabs.ContainsKey(prefabID): false;

		public static void init(PrefabsConfig config)
		{
			prefabsConfig = config;
			refreshValidPrefabs(false);
		}

		public static void refreshValidPrefabs(bool searchForDebris)
		{
			Common.Debug.assert(prefabsConfig != null);

			validPrefabs = prefabsConfig.getValidPrefabs();

			if (!searchForDebris)
				return;

			foreach (var pid in Object.FindObjectsOfType<PrefabIdentifier>())
			{
				if (isValidPrefab(pid.ClassId))
				{
					DebrisTracker.track(pid.gameObject);
				}
				else
				{
					if (pid.gameObject.GetComponent<DebrisDeconstructable>())
						unpatchObject(pid.gameObject, true);

					if (pid.gameObject.GetComponent<ResourceTracker>()?.overrideTechType == SalvageableDebrisDR.TechType)
						DebrisTracker.untrack(pid.gameObject);
				}
			}

			DebrisTracker.untrackInvalid();											$"DebrisPatcher: prefabs refreshed ({validPrefabs.Count} valid prefabs)".logDbg();
		}


		public static void processObject(GameObject go)
		{
			if (!isValidForPatching(go))
				return;

			if (Main.config.customObjects.hotkeysEnabled)
				processCustomObject(go);

			tryPatchObject(go);
		}


		public static void processCustomObject(GameObject go)
		{
			PrefabsConfig.PrefabList targetList = null;

			if (Input.GetKey(Main.config.customObjects.hotkey))
				targetList = prefabsConfig.dbsCustom;
			else
			if (Input.GetKey(Main.config.customObjects.hotkeyTemp))
				targetList = prefabsConfig.dbsCustomTemp;

			if (targetList == null)
				return;

			var prefabID = go.getComponentInHierarchy<PrefabIdentifier>(false);

			if (prefabID == null || isValidPrefab(prefabID.ClassId))
				return;

			string prefabName = targetList.addPrefab(prefabID.ClassId, Main.config.customObjects.defaultResourceCount);
			prefabsConfig.save();
			refreshValidPrefabs(true);

			L10n.str(L10n.ids_customDebrisAdded).format(prefabName).onScreen();
		}


		public static void unpatchObject(GameObject go, bool removeDebrisCmp)
		{
			if (go.GetComponent<Constructable>()?.constructed == false) // don't unpatch partially deconstructed objects
				return;
																					$"DebrisPatcher: unpatching object {go.name}".logDbg();
			go.destroyComponent<Constructable>(false);

			if (removeDebrisCmp)
				go.destroyComponent<DebrisDeconstructable>(false);

			DebrisTracker.untrack(go);
		}


		static bool isValidForPatching(GameObject go)
		{
			// checking both DebrisDeconstructable and Constructable because we can delete Constructable later in special processing
			if (!go || go.getComponentInHierarchy<DebrisDeconstructable>(false) || go.getComponentInHierarchy<Constructable>(false))
				return false;

			if (Main.config.patchStaticObjects)
				return true;

			return go.getComponentInHierarchy<Rigidbody>(false)?.isKinematic == false; // and if object movable
		}


		static void tryPatchObject(GameObject go)
		{
			var prefabID = go.getComponentInHierarchy<PrefabIdentifier>(false);

			if (prefabID && validPrefabs.TryGetValue(prefabID.ClassId, out int resourcesCount))
			{
				addConstructableComponent(prefabID.gameObject, resourcesCount);
				DebrisSpecialProcess.tryProcessSpecial(prefabID);
			}
		}


		static void addConstructableComponent(GameObject go, int resourcesCount)
		{																						$"GameObject '{go.name} already have Constructable!'".logDbgError(go.GetComponent<Constructable>());
			go.AddComponent<DebrisDeconstructable>();

			var constructable = go.AddComponent<Constructable>();

			constructable.model = go.GetComponentInChildren<MeshRenderer>()?.gameObject;

			constructable.resourceMap = new List<TechType>();
			constructable.resourceMap.add(TechType.ScrapMetal, resourcesCount / 10);
			constructable.resourceMap.add(ScrapMetalSmall.TechType, resourcesCount % 10);		$"Constructable added to {go.name}".logDbg();
		}
	}
}