using System.Collections.Generic;
using UnityEngine;
using Common;

namespace DebrisRecycling
{
	class DebrisDeconstructable: MonoBehaviour, IConstructable
	{
		public bool CanDeconstruct(out string reason)
		{
			reason = "Try to move object";

			if (Main.config.deconstructValidStaticObjects)
				return true;
			
			Rigidbody rigidbody = gameObject.getComponentInHierarchy<Rigidbody>(true, false);
			return (rigidbody && !rigidbody.isKinematic);
		}

		public void OnConstructedChanged(bool constructed) {}
	}


	static class DebrisPatcher
	{
		static readonly Dictionary<string, int> validPrefabs = new Dictionary<string, int>();
		static bool inited = false;

		public static void init(ModConfig.PrefabsConfig prefabsConfig, PrefabIDs prefabIDs)
		{
			if (!inited)
			{
				inited = true;

				validPrefabs.addRange(prefabIDs.debrisCargoOpened);
				validPrefabs.addRange(prefabIDs.debrisMiscMovable);

				if (prefabsConfig.includeFurniture)
					validPrefabs.addRange(prefabIDs.debrisFurniture);

				if (prefabsConfig.includeLockers)
					validPrefabs.addRange(prefabIDs.debrisLockers);

				if (prefabsConfig.includeTech)
					validPrefabs.addRange(prefabIDs.debrisTech);

				if (prefabsConfig.includeClosedCargo)
					validPrefabs.addRange(prefabIDs.debrisCargoClosed);

#if !EXCLUDE_STATIC_DEBRIS
				if (prefabsConfig.includeBigStatic)
					validPrefabs.addRange(prefabIDs.debrisStatic);
#endif
				$"Debris patcher inited, prefabs id count:{validPrefabs.Count}".logDbg();
			}
		}


		public static void processObject(GameObject go)
		{
			if (isValidForPatching(go))
			{
				if (Main.config.hotkeyForNewObjects)
					updateHotkeys(go);
				
				tryPatchObject(go);
			}
		}


		static bool isValidForPatching(GameObject go)
		{
			// checking both DebrisDeconstructable and Constructable because we can delete Constructable later in special processing
			if (go && !go.getComponentInHierarchy<DebrisDeconstructable>(false) && !go.getComponentInHierarchy<Constructable>(false))
			{
				if (Main.config.patchStaticObjects)
					return true;

				Rigidbody rigidbody = go.getComponentInHierarchy<Rigidbody>(false);
				
				if (rigidbody && !rigidbody.isKinematic) // and if object movable
					return true;
			}
			
			return false;
		}


		static void tryPatchObject(GameObject go)
		{
			PrefabIdentifier prefabID = go.getComponentInHierarchy<PrefabIdentifier>(false);

			if (prefabID)
			{
				if (validPrefabs.TryGetValue(prefabID.ClassId, out int resourcesCount))
				{
					addConstructableComponent(prefabID.gameObject, resourcesCount);
					DebrisSpecialProcess.tryProcessSpecial(prefabID);
				}
			}
		}


		static void addConstructableComponent(GameObject go, int resourcesCount)
		{																						$"GameObject '{go.name} already have Constructable!'".logDbgError(go.GetComponent<Constructable>());
			go.AddComponent<DebrisDeconstructable>();

			Constructable constructable = go.AddComponent<Constructable>();

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
				PrefabIdentifier prefabID = go.getComponentInHierarchy<PrefabIdentifier>(false);

				if (prefabID == null)
					return;

				$"-----id:{prefabID.Id} classID:{prefabID.ClassId} name:{prefabID.gameObject.name}".log();

				if (!validPrefabs.ContainsKey(prefabID.ClassId))
				{
					validPrefabs.Add(prefabID.ClassId, 10);
					dbgPrefabs.Add(prefabID.ClassId, 10);
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