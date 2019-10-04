using Harmony;

using Common.Config;

namespace WarningsDisabler
{
	// remove low oxygen warnings
	static class OxygenWarnings
	{
		static int hintMessageHash = 0;
		
		public class HideOxygenHint: Options.IFieldCustomAction
		{
			public void fieldCustomAction()
			{
				if (!Main.config.oxygenWarningsEnabled)
				{
					uGUI_PopupMessage popup = Hint.main?.message;
					
					if (popup && popup.isShowingMessage && popup.messageHash == hintMessageHash)
						popup.Hide();
				}
			}
		}
		
		// for hiding popup message properly when changing option in game
		[HarmonyPatch(typeof(HintSwimToSurface), "OnLanguageChanged")]
		static class HintSwimToSurface_OnLanguageChanged_Patch
		{
			static void Postfix(HintSwimToSurface __instance) => hintMessageHash = __instance.messageHash;
		}
		
		[HarmonyPatch(typeof(HintSwimToSurface), "Update")]
		static class HintSwimToSurface_Update_Patch
		{
			static bool Prefix(HintSwimToSurface __instance) => Main.config.oxygenWarningsEnabled;
		}
	
		[HarmonyPatch(typeof(LowOxygenAlert), "Update")]
		static class LowOxygenAlert_Update_Patch
		{
			static bool Prefix(LowOxygenAlert __instance) => Main.config.oxygenWarningsEnabled;
		}
	}
}
