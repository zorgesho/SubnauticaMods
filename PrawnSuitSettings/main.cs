using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	public static class Main
	{
		internal static ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			Options.init();

			if (config.armsEnergyUsage.usageEnabled)
				ArmsEnergyUsage.refresh();
		}
	}
}