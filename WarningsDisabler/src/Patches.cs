using System;

using Harmony;

using Common;
using Common.Configuration;

namespace WarningsDisabler
{
	[HarmonyPatch(typeof(PDANotification), "Play", new Type[] { typeof(object[]) })]
	static class PDANotification_Play_Patch
	{
		static bool Prefix(PDANotification __instance)
		{																							$"PDANotification.Play {__instance.text}".onScreen().logDbg();
			return Main.config.isMessageAllowed(__instance.text);
		}
	}


	[HarmonyPatch(typeof(VoiceNotification), "Play", new Type[] { typeof(object[]) })]
	static class VoiceNotification_Play_Patch
	{
		static bool Prefix(VoiceNotification __instance)
		{																							$"VoiceNotification.Play {__instance.text}, interval:{__instance.minInterval}".onScreen().logDbg();
			return Main.config.isMessageAllowed(__instance.text);
		}
	}


	// Disabling low oxygen warnings
	static class OxygenWarnings
	{
		static int hintMessageHash = 0;

		// for hiding popup message when changing option in game
		public class HideOxygenHint: Config.Field.IAction
		{
			public void action()
			{
				if (Main.config.oxygenWarningsEnabled)
					return;

				uGUI_PopupMessage popup = Hint.main?.message;

				if (popup && popup.isShowingMessage && popup.messageHash == hintMessageHash)
					popup.Hide();
			}
		}

		// to make sure we hide proper popup
		[HarmonyPatch(typeof(HintSwimToSurface), "OnLanguageChanged")]
		static class HintSwimToSurface_OnLanguageChanged_Patch
		{
			static void Postfix(HintSwimToSurface __instance) => hintMessageHash = __instance.messageHash;
		}

		[HarmonyPatch(typeof(HintSwimToSurface), "Update")]
		static class HintSwimToSurface_Update_Patch
		{
			static bool Prefix() => Main.config.oxygenWarningsEnabled;
		}

		[HarmonyPatch(typeof(LowOxygenAlert), "Update")]
		static class LowOxygenAlert_Update_Patch
		{
			static bool Prefix() => Main.config.oxygenWarningsEnabled;
		}
	}
}