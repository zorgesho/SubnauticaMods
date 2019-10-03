using Harmony;

using Common.Config;

namespace WarningsDisabler
{
	// remove all oxygen warnings
	static class OxygenWarnings
	{
		static int hintMessageHash = 0;
		
		public class OxygenWarningsCustomAction: Options.IFieldCustomAction
		{
			public void fieldCustomAction()
			{
				if (Main.config.disableOxygenWarnings)
				{
					uGUI_PopupMessage popup = Hint.main?.message;
					
					if (popup && popup.isShowingMessage && popup.messageHash == hintMessageHash)
						popup.Hide();
				}
			}
		}
		
		// for hiding popup message properly when changing option in game
		[HarmonyPatch(typeof(HintSwimToSurface), "OnLanguageChanged")]
		class HintSwimToSurface_OnLanguageChanged_Patch
		{
			static void Postfix(HintSwimToSurface __instance)
			{
				hintMessageHash = __instance.messageHash;
			}
		}
		
		[HarmonyPatch(typeof(HintSwimToSurface), "Update")]
		class HintSwimToSurface_Update_Patch
		{
			static bool Prefix(HintSwimToSurface __instance)
			{
				return !Main.config.disableOxygenWarnings;
			}
		}
	
		[HarmonyPatch(typeof(LowOxygenAlert), "Update")]
		class LowOxygenAlert_Update_Patch
		{
			static bool Prefix(LowOxygenAlert __instance)
			{
				return !Main.config.disableOxygenWarnings;
			}
		}
	}
}
