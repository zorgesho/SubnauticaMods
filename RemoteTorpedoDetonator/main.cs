using Common;
using Common.Crafting;
using Common.Configuration;

namespace RemoteTorpedoDetonator
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			LanguageHelper.init();

			HarmonyHelper.patchAll(true);
			CraftHelper.patchAll();
		}
	}
}