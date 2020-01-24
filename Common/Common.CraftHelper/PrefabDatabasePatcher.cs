using System.Linq;
using System.Collections.Generic;

using UWE;
using Harmony;
using UnityEngine;

namespace Common.Crafting
{
	static class PrefabDatabasePatcher
	{
		static Dictionary<string, CraftableObject> prefabs = null;

		public static void addPrefab(CraftableObject craftableObject)
		{
			if (prefabs == null)
			{
				prefabs = new Dictionary<string, CraftableObject>();
				HarmonyHelper.patch();
			}

			prefabs[craftableObject.PrefabFileName] = craftableObject;
		}

		[HarmonyPatch(typeof(PrefabDatabase), "GetPrefabForFilename")][HarmonyPrefix]
		static bool getPrefabForFilename(string filename, ref GameObject __result)
		{																										$"PrefabDatabasePatcher.getPrefabForFilename: {filename}".logDbg();
			if (prefabs.TryGetValue(filename, out CraftableObject co))
			{
				__result = co.getGameObject();																	$"PrefabDatabasePatcher.getPrefabForFilename: using exact prefab {filename}".logDbg();
				return false;
			}

			return true;
		}
#if DEBUG
		// just for debug for now
		[HarmonyPatch(typeof(PrefabDatabase), "GetPrefabAsync")][HarmonyPostfix]
		static void getPrefabAsync(string classId)
		{
			if (!prefabs.FirstOrDefault(p => p.Value.ClassID == classId).Equals(default(KeyValuePair<string, CraftableObject>)))
				$"PrefabDatabasePatcher.getPrefabAsync: {classId}".logError();
		}
#endif
	}
}