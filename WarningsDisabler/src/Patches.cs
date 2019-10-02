using System;
using Harmony;
using UnityEngine;
using Common;

namespace WarningsDisabler
{
	// remove all oxygen warnings
	class OxygenWarnings
	{	
		[HarmonyPatch(typeof(HintSwimToSurface), "Update")]
		class HintSwimToSurface_Update_Patch
		{
			private static bool Prefix(HintSwimToSurface __instance)
			{
				return !Main.config.disableOxygenWarnings;
			}
		}
	
		[HarmonyPatch(typeof(LowOxygenAlert), "Update")]
		class LowOxygenAlert_Update_Patch
		{
			private static bool Prefix(LowOxygenAlert __instance)
			{
				return !Main.config.disableOxygenWarnings;
			}
		}
	}
}
