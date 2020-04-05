using Common;
using Common.Crafting;
using Common.Configuration;

namespace DebrisRecycling
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			CraftHelper.patchAll();

			LanguageHelper.init();

			DebrisPatcher.init(config.prefabsConfig, Config.tryLoad<PrefabIDs>("prefabs_config.json", Config.LoadOptions.None));
		}
	}
}