using Common;
using Common.Configuration;

namespace ModsOptionsAdjusted
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll(true);
		}
	}
}