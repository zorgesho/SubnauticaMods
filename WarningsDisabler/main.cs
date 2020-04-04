using Common;

namespace WarningsDisabler
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			LanguageHelper.init();
		}
	}
}