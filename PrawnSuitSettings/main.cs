using Common;

namespace PrawnSuitSettings
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			if (config.armsEnergyUsage.enabled)
				ArmsEnergyUsage.refresh();
		}
	}
}