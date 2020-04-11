using Common;
using Common.Crafting;

namespace PrawnSuitGrapplingArmUpgrade
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			CraftHelper.patchAll();
		}
	}
}