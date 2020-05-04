using Harmony;

namespace MiscPatches
{
	// close all doors in cyclops at start
	[HarmonyPatch(typeof(SubRoot), "Start")]
	static class SubRoot_Start_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(SubRoot __instance)
		{
			if (!__instance.isBase) // we're in cyclops
			{
				foreach (var door in __instance.gameObject.GetAllComponentsInChildren<Openable>())
					door.Invoke("Close", 0.5f);
			}
		}
	}


	// hide red "Engines offline" label from cyclops hud
	[HarmonyPatch(typeof(CyclopsHelmHUDManager), "Update")]
	static class CyclopsHelmHUDManager_Update_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(CyclopsHelmHUDManager __instance)
		{
			if (__instance.LOD.IsFull() && __instance.engineOffText.gameObject.activeSelf)
				__instance.engineOffText.gameObject.SetActive(false);
		}
	}
}