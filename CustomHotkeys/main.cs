using Common;
using Common.Configuration;

namespace CustomHotkeys
{
	public static class Main
	{
		internal static ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			HotkeyHelper.init();
		}
	}
}