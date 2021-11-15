#if GAME_SN
using HarmonyLib;
using UnityEngine.Events;

using Common;
using Common.Harmony;

namespace MiscPatches
{
	[PatchClass]
	static class CyclopsPatches
	{
		static bool prepare() => Main.config.gameplayPatches;

		// close all doors in cyclops at start
		[HarmonyPostfix, HarmonyPatch(typeof(SubRoot), "Start")]
		static void SubRoot_Start_Postfix(SubRoot __instance)
		{
			if (!__instance.isBase) // we're in cyclops
				__instance.GetComponentsInChildren<Openable>().ForEach(door => door.Invoke("Close", 0.5f));
		}

		// hide red "Engines offline" label from cyclops hud
		[HarmonyPostfix, HarmonyPatch(typeof(CyclopsHelmHUDManager), "Update")]
		static void CyclopsHelmHUDManager_Update_Postfix(CyclopsHelmHUDManager __instance)
		{
			if (__instance.LOD.IsFull() && __instance.engineOffText.gameObject.activeSelf)
				__instance.engineOffText.gameObject.SetActive(false);
		}

		// fix hatch and antennas for docked vehicles in cyclops
		// playing vehicle dock animation after load, didn't find another way
		// exosuit is also slightly moved from cyclops dock bay hatch, need to play all docking animations to fix it (like in moonpool)
		[HarmonyPostfix, HarmonyPatch(typeof(Vehicle), "Start")]
		static void Vehicle_Start_Postfix(Vehicle __instance)
		{
			const float delay = 7f;

			if (!__instance.docked || __instance.GetComponentInParent<SubRoot>()?.isBase != false)
				return;

			// we're docked in cyclops
			(__instance as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", true);

			__instance.gameObject.callAfterDelay(delay, new UnityAction(() =>
			{
				(__instance as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", false);
			}));
		}
	}
}
#endif