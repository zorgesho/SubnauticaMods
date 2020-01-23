using Common;
using Common.Crafting;
using Common.Configuration;

namespace DebrisRecycling
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			Options.init();
			CraftHelper.patchAll();

			DebrisPatcher.init(config.prefabsConfig, Config.tryLoad<PrefabIDs>("prefabs_config.json", Config.LoadOptions.None));
		}
	}
}