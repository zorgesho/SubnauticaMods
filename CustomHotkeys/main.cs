using Common;
using Common.Configuration;

namespace CustomHotkeys
{
	static public class Main
	{
		static internal ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();

			HotkeyHelper.init();
		}
	}
}