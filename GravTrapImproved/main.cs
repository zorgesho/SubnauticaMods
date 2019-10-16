using Common;
using Common.Config;

namespace GravTrapImproved
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