using Common;

namespace ConsoleImproved
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			if (config.fixVanillaCommandsFloatParse)
				CommandsFloatParsePatch.patchAll();
		}
	}
}