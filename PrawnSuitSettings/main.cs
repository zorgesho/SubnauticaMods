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

			if (config.addOptionsToMenu)
				Options.init();

			if (config.armsEnergyUsage.enabled)
				ArmsEnergyUsage.refresh();

			if (!config.autoPickupDrillableResources)
				DrillableResourcesPickup.refresh();
		}
	}
}