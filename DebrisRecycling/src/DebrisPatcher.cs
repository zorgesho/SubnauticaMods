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

			if (Main.config.hotkeyForNewObjects)
				updateHotkeys(go);

			tryPatchObject(go);
		}


		public static void unpatchObject(GameObject go, bool removeDebrisCmp)
		{																			$"DebrisPatcher: unpatching object {go.name}".logDbg();
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


		static readonly Dictionary<string, int> dbgPrefabs = new Dictionary<string, int>();

		static void updateHotkeys(GameObject go)
		{
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				var prefabID = go.getComponentInHierarchy<PrefabIdentifier>(false);

				if (prefabID == null)
					return;

				$"-----id:{prefabID.Id} classID:{prefabID.ClassId} name:{prefabID.gameObject.name}".log();

				if (!isValidPrefab(prefabID.ClassId))
				{
					validPrefabs[prefabID.ClassId] = 10;
					dbgPrefabs[prefabID.ClassId] = 10;
				}

				string prefabs = "-----dbgPrefabs\r\n";
				foreach (KeyValuePair<string, int> v in dbgPrefabs)
					prefabs += $"{{\"{v.Key}\", {v.Value}}},\r\n";

				prefabs.log();
				"-----".log();
			}
		}
	}
}