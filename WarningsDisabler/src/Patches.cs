using Harmony;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace WarningsDisabler
{
	[HarmonyPatch(typeof(PDANotification), "Play", typeof(object[]))]
	static class PDANotification_Play_Patch
	{
		static bool Prefix(PDANotification __instance)
		{																							$"PDANotification.Play {__instance.text}".onScreen().logDbg();
			return Main.config.isMessageAllowed(__instance.text);
		}
	}

	[HarmonyPatch(typeof(VoiceNotification), "Play", typeof(object[]))]
	static class VoiceNotification_Play_Patch
	{
		static bool Prefix(VoiceNotification __instance)
		{																							$"VoiceNotification.Play {__instance.text}, interval:{__instance.minInterval}".onScreen().logDbg();
			return Main.config.isMessageAllowed(__instance.text);
		}
	}

	// Disabling low oxygen warnings
	[PatchClass]
	static class OxygenWarnings
	{
		// for hiding popup message when changing option in game
		public class HideOxygenHint: Config.Field.IAction
		{
			public void action()
			{
				if (!Main.config.oxygenWarningsEnabled)
					Hint.main?.message.Hide();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(LowOxygenAlert), "Update")]
		[HarmonyPatch(typeof(HintSwimToSurface), "Update")]
		static bool OxygenAlert_Prefix() => Main.config.oxygenWarningsEnabled;
	}

#if GAME_BZ
	[OptionalPatch, PatchClass]
	static class DisclaimerPatches
	{
		static bool prepare() => !Main.config.showDisclaimers;

		[HarmonyPrefix, HarmonyPatch(typeof(EarlyAccessDisclaimer), "OnEnable")]
		static bool EarlyAccessDisclaimer_OnEnable_Prefix(EarlyAccessDisclaimer __instance)
		{
			UnityEngine.Object.Destroy(__instance.gameObject);
			return false;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(uGUI_MainMenu), "OnButtonLoad")]
		static bool uGUIMainMenu_OnButtonLoad_Prefix(uGUI_MainMenu __instance)
		{
			__instance.rightSide.OpenGroup("SavedGames");
			return false;
		}
	}
#endif
}