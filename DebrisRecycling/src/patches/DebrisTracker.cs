using Harmony;

namespace DebrisRecycling
{
	[HarmonyPatch(typeof(UniqueIdentifier), "Awake")]
	static class UniqueIdentifier_Awake_Patch
	{
		static bool Prepare() => Main.config.addDebrisToScannerRoom;

		static void Postfix(UniqueIdentifier __instance)
		{
			if (!DebrisPatcher.isValidPrefab(__instance.ClassId))
				return;

			var rt = __instance.gameObject.AddComponent<ResourceTracker>();
			rt.prefabIdentifier = __instance as PrefabIdentifier;
			rt.overrideTechType = SalvageableDebrisDR.TechType;
			rt.Register();
		}
	}
}