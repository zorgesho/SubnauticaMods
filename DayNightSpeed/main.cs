using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll(true);

			Options.init();

			DayNightSpeedControl.init();
			DayNightCyclePatches.init();
		}
	}
}