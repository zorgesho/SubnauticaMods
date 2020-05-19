using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.Reflection;

namespace DebrisRecycling
{
	static class DebrisTracker
	{
		// key - uniqueID, value - classID
		static readonly Dictionary<string, string> trackedDebris = new Dictionary<string, string>();

		public static void track(GameObject go)
		{
			if (!Main.config.addDebrisToScannerRoom)
				return;

			if (go.GetComponent<ResourceTracker>())
				return;

			var pid = go.GetComponent<PrefabIdentifier>();

			if (!pid)
				return;

			var rt = go.AddComponent<ResourceTracker>();
			rt.overrideTechType = SalvageableDebrisDR.TechType;
			rt.prefabIdentifier = pid;

			trackedDebris[pid.Id] = pid.ClassId;
			rt.Register();
		}


		public static void untrack(GameObject go) => untrack(go.GetComponent<ResourceTracker>());

		static void untrack(ResourceTracker rt)
		{
			if (!rt)
				return;
																					$"DebrisTracker: object untracked {rt.name}".logDbg();
			trackedDebris.Remove(rt.prefabIdentifier.Id);

			rt.Unregister();
			Object.Destroy(rt);
		}


		public static void untrackInvalid()
		{
			if (!ResourceTracker.resources.TryGetValue(SalvageableDebrisDR.TechType, out var trackedResources))
				return;

			var toRemove = new List<ResourceTracker.ResourceInfo>();

			foreach (var info in trackedResources)
			{
				if (trackedDebris.TryGetValue(info.Key, out string prefabID) && !DebrisPatcher.isValidPrefab(prefabID))
					toRemove.Add(info.Value);
			}

			var onResourceRemoved = typeof(ResourceTracker).evnt("onResourceRemoved").wrap(); // don't make it static

			foreach (var info in toRemove)
			{
				trackedResources.Remove(info.uniqueId);
				onResourceRemoved.raise(info);
			}
		}


		[HarmonyPatch(typeof(UniqueIdentifier), "Awake")]
		static class UniqueIdentifier_Awake_Patch
		{
			static bool Prepare() => Main.config.addDebrisToScannerRoom;

			static void Postfix(UniqueIdentifier __instance)
			{
				if (__instance is PrefabIdentifier && DebrisPatcher.isValidPrefab(__instance.ClassId))
					track(__instance.gameObject);
			}
		}
	}
}