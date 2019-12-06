using Common;
using Common.Crafting;
using Common.Configuration;

namespace OxygenRefill
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			CraftHelper.patchAll();
		}
	}
}