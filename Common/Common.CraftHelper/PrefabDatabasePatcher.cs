using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UWE;

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
				patchPrefabDatabase();
			}

			prefabs[craftableObject.PrefabFileName] = craftableObject;
		}

		// prefix for PrefabDatabase.GetPrefabForFilename
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
		// postfix for PrefabDatabase.GetPrefabAsync, just for debug for now
		static void getPrefabAsync(string classId)
		{
			if (!prefabs.FirstOrDefault(p => p.Value.ClassID == classId).Equals(default(KeyValuePair<string, CraftableObject>)))
				$"PrefabDatabasePatcher.getPrefabAsync: {classId}".logError();
		}
#endif

		static void patchPrefabDatabase()
		{
			HarmonyHelper.patch(typeof(PrefabDatabase).method("GetPrefabForFilename"),
				prefix: typeof(PrefabDatabasePatcher).method(nameof(PrefabDatabasePatcher.getPrefabForFilename)));
#if DEBUG
			HarmonyHelper.patch(typeof(PrefabDatabase).method("GetPrefabAsync"),
				postfix: typeof(PrefabDatabasePatcher).method(nameof(PrefabDatabasePatcher.getPrefabAsync)));
#endif
		}
	}
}