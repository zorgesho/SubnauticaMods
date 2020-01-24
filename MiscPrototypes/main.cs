using Common;
using Common.Crafting;
using Common.Configuration;

namespace MiscPrototypes
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll(true);
#if TEST_MOD_OPTIONS
			Options.init();
#endif
			CraftHelper.patchAll();
		}
	}
}