using Common;
using Common.Configuration;

namespace ConsoleImproved
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			if (config.setInvariantCultureAppWide)
				System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

			HarmonyHelper.patchAll();

			if (config.fixVanillaCommandsFloatParse)
				CommandsFloatParsePatch.patchAll();
		}
	}
}