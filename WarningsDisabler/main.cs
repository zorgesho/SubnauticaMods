using Common;
using Common.Configuration;

namespace WarningsDisabler
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			if (config.addOptionsToMenu)
				Options.init();
		}
	}
}