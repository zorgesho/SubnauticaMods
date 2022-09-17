using HarmonyLib;

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
	// Disabling disclaimer on startup
	[HarmonyPatch(typeof(FlashingLightsDisclaimer), "Start")]
	static class FlashingLightsDisclaimer_Start_Patch
	{
		static bool Prepare() => !Main.config.showDisclaimer;

		static bool Prefix(FlashingLightsDisclaimer __instance)
		{
			__instance.gameObject.SetActive(false);
			return false;
		}
	}
#endif
}