using Common;
using Common.Crafting;
using Common.Configuration;

namespace DebrisRecycling
{
	public static class Main
	{
		internal const string prefabsConfigName = "prefabs_config.json";

		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			CraftHelper.patchAll();

			LanguageHelper.init(); // after CraftHelper

			DebrisPatcher.init(Mod.loadConfig<PrefabsConfig>(prefabsConfigName, Config.LoadOptions.ProcessAttributes));
		}
	}
}