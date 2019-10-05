using System;
using System.Collections.Generic;

using Harmony;

using Common;

namespace WarningsDisabler
{
	// Disabling power warnings and welcome messages
	[HarmonyPatch(typeof(VoiceNotification), "Play", new Type[] { typeof(object[]) })]
	static class VoiceNotification_Play_Patch
	{
		static List<string> powerWarnings = new List<string>()
		{
			"BasePowerUp",			// "HABITAT: Power restored. All primary systems online."
			"BasePowerDown",		// "HABITAT: Warning, emergency power only."
			"BaseWelcomeNoPower"	// "HABITAT: Warning: Emergency power only. Oxygen production offline."
		};

		static List<string> welcomeMessages = new List<string>()
		{
			"CyclopsWelcomeAboard",				// "CYCLOPS: Welcome aboard captain. All systems online."
			"CyclopsWelcomeAboardAttention",	// "CYCLOPS: Welcome aboard captain. Some systems require attention."
			"SeamothWelcomeAboard",				// "Seamoth: Welcome aboard captain."
			"SeamothWelcomeNoPower",			// "Seamoth: Warning: Emergency power only. Oxygen production offline."
			"ExosuitWelcomeAboard",				// "PRAWN: Welcome aboard captain."
			"ExosuitWelcomeNoPower",			// "PRAWN: Warning: Emergency power only. Oxygen production offline."
			"BaseWelcomeAboard"					// "HABITAT: Welcome aboard captain."
			//"BaseWelcomeNoPower"				//  Moved to powerWarnings list
		};
		
		static bool Prefix(VoiceNotification __instance, object[] args, bool __result)
		{																											$"VoiceNotification.Play {__instance.text}, interval:{__instance.minInterval}".onScreen().logDbg();
			if (!Main.config.powerWarningsEnabled && powerWarnings.Find((s) => __instance.text == s) != null)
				return false;
			
			if (!Main.config.welcomeMessagesEnabled && welcomeMessages.Find((s) => __instance.text == s) != null)
				return false;

			return true;
		}
	}
}