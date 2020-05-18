using Common;
using Common.Harmony;

namespace DayNightSpeed
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll(true);

			LanguageHelper.init();

			DayNightSpeedControl.init();
			DayNightCyclePatches.init();
		}
	}
}