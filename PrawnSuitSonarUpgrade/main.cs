using Common;
using Common.Harmony;
using Common.Crafting;

namespace PrawnSuitSonarUpgrade
{
	public static class Main
	{
		public static void patch()
		{
			Mod.init();

			HarmonyHelper.patchAll();
			CraftHelper.patchAll();
		}
	}
}