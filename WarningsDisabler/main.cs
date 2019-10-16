using Common;
using Common.Config;

namespace WarningsDisabler
{
	static public class Main
	{
		static internal readonly ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();

			Options.init();
		}
	}
}