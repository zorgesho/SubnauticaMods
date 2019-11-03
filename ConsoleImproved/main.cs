using Common;
using Common.Configuration;

namespace ConsoleImproved
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
		}
	}
}