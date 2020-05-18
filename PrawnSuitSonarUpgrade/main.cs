using Common.Harmony;
using Common.Crafting;

namespace PrawnSuitSonarUpgrade
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.patchAll();
			CraftHelper.patchAll();
		}
	}
}