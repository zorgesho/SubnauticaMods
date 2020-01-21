using Harmony;

namespace ModsOptionsAdjusted
{
	[HarmonyPatch(typeof(uGUI_OptionsPanel), "AddTab")]
	static class uGUIOptionsPanel_AddTab_Patch
	{
		public static int modsTabIndex { get; private set; } = -1;
		public static bool isMainMenu { get; private set; } = true; // is options opened in main menu or in game

		static void Postfix(uGUI_OptionsPanel __instance, string label, int __result)
		{
			if (label == "Mods")
				modsTabIndex = __result;

			isMainMenu = (__instance.GetComponent<MainMenuOptions>() != null);
		}
	}
}