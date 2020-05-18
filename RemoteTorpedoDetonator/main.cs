using Common;
using Common.Harmony;
using Common.Crafting;

namespace RemoteTorpedoDetonator
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			LanguageHelper.init();

			HarmonyHelper.patchAll(true);
			CraftHelper.patchAll();
		}
	}
}