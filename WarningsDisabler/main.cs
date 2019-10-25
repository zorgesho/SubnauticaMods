using Common;
using Common.Configuration;

namespace WarningsDisabler
{
	static public class Main
	{
		static internal readonly ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();

			if (config.addOptionsToMenu)
				Options.init();
		}
	}
}