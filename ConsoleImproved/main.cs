using Common;
using Common.Config;

namespace ConsoleImproved
{
	static public class Main
	{
		static internal readonly ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();
		}
	}
}