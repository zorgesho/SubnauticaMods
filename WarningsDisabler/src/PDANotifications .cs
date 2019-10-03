using System;
using System.Collections.Generic;

using Harmony;

using Common;

namespace WarningsDisabler
{
	[HarmonyPatch(typeof(PDANotification), "Play", new Type[] { typeof(object[]) })]
	static class PDANotification_Play_Patch
	{
		static List<string> depthWarnings = new List<string>()
		{
			"DepthWarning100",	// "Warning: Passing 100 meters. Oxygen efficiency decreased."
			"DepthWarning200"	// "Warning: Passing 200 meters. Oxygen efficiency greatly decreased."
		};

		static List<string> foodWaterWarnings = new List<string>()
		{
			"VitalsOk",			// "Vital signs stabilizing."
			"FoodLow",			// "Calorie intake recommended."
			"FoodCritical",		// "Emergency, starvation imminent. Seek calorie intake immediately."
			"FoodVeryLow",		// "Seek calorie intake."
			"WaterLow",			// "Seek fluid intake."
			"WaterCritical",	// "Seek fluid intake immediately."
			"WaterVeryLow"		// "Seek fluid intake."
		};

		static bool Prefix(PDANotification __instance, object[] args)
		{																											$"PDANotification.Play {__instance.text}".onScreen().logDbg();
			if (Main.config.disableDepthWarnings && depthWarnings.Find((s) => __instance.text == s) != null)
				return false;
			
			if (Main.config.disableFoodWaterWarnings && foodWaterWarnings.Find((s) => __instance.text == s) != null)
				return false;

			return true;
		}
	}
}