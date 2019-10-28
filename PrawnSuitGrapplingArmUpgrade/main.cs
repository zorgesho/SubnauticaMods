using Common;
using Common.Configuration;

namespace PrawnSuitGrapplingArmUpgrade
{
	static public class Main
	{
		static internal ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();
		}
	}
}