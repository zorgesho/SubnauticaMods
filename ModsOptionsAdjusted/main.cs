using Common;
using Common.Configuration;

namespace ModsOptionsAdjusted
{
	public static class Main
	{
#if DEBUG
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>(); // for tests
#endif
		public static void patch()
		{
			HarmonyHelper.patchAll();
#if DEBUG
			Options.init(); // for tests
#endif
		}
	}
}