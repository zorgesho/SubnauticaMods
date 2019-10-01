using System;
using Harmony;
using UnityEngine;

namespace WarningsDisabler
{
	// remove all oxygen warnings
	class OxygenWarnings
	{	
		[HarmonyPatch(typeof(HintSwimToSurface))]
		[HarmonyPatch("Update")]
		class HintSwimToSurface_Update_Patch
		{
			private static bool Prefix(HintSwimToSurface __instance)
			{
				GameObject.Destroy(__instance);
				return false;
			}
		}
	
		[HarmonyPatch(typeof(LowOxygenAlert))]
		[HarmonyPatch("Update")]
		class LowOxygenAlert_Update_Patch
		{
			private static bool Prefix(LowOxygenAlert __instance)
			{
				GameObject.Destroy(__instance);
				return false;
			}
		}
	}
}
