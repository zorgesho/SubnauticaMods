using System;
using System.Collections.Generic;

using Harmony;

using Common;

namespace WarningsDisabler
{
	[HarmonyPatch(typeof(VoiceNotification), "Play", new Type[] { typeof(object[]) })]
	static class VoiceNotification_Play_Patch
	{
		static List<string> powerMessages = new List<string>()
		{
			"BasePowerUp",		// "HABITAT: Power restored. All primary systems online."
			"BasePowerDown"		// "HABITAT: Warning, emergency power only."
		};

		static List<string> welcomeMessages = new List<string>()
		{
			"CyclopsWelcomeAboard",				// "CYCLOPS: Welcome aboard captain. All systems online."
			"CyclopsWelcomeAboardAttention",	// "CYCLOPS: Welcome aboard captain. Some systems require attention."
			"SeamothWelcomeAboard",				// "Seamoth: Welcome aboard captain."
			"SeamothWelcomeNoPower",			// "Seamoth: Warning: Emergency power only. Oxygen production offline."
			"ExosuitWelcomeAboard",				// "PRAWN: Welcome aboard captain."
			"ExosuitWelcomeNoPower",			// "PRAWN: Warning: Emergency power only. Oxygen production offline."
			"BaseWelcomeAboard",				// "HABITAT: Welcome aboard captain."
			"BaseWelcomeNoPower"				// "HABITAT: Warning: Emergency power only. Oxygen production offline."
		};
		
		static bool Prefix(VoiceNotification __instance, object[] args, bool __result)
		{																											$"VoiceNotification.Play {__instance.text}, interval:{__instance.minInterval}".onScreen().logDbg();
			if (Main.config.disablePowerWarnings && powerMessages.Find((s) => __instance.text == s) != null)
				return false;
			
			if (Main.config.disableWelcomesMessages && welcomeMessages.Find((s) => __instance.text == s) != null)
				return false;

			return true;
		}
	}

	
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