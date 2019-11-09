using Common;

namespace PrawnSuitSonarUpgrade
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.patchAll(false);

			PrawnSonarModule.patch();
		}
	}
}