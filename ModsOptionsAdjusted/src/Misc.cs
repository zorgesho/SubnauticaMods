using HarmonyLib;
using Common.Harmony;

namespace ModsOptionsAdjusted
{
	[PatchClass]
	static class OptionsPanelInfo
	{
		public static int modsTabIndex { get; private set; } = -1;
		public static bool isMainMenu  { get; private set; } = true; // is options opened in main menu or in game

		[HarmonyPostfix, HarmonyPatch(typeof(uGUI_OptionsPanel), "AddTab")]
		static void _addTab(uGUI_OptionsPanel __instance, string label, int __result)
		{
			if (label == "Mods")
				modsTabIndex = __result;

			isMainMenu = (__instance.GetComponent<MainMenuOptions>() != null);
		}
	}
}