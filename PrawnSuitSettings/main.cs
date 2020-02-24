using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			if (config.armsEnergyUsage.enabled)
				ArmsEnergyUsage.refresh();
		}
	}
}