using Common;
using Common.Harmony;
using Common.Crafting;
using Common.Configuration;

namespace GravTrapImproved
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			LanguageHelper.init();

			HarmonyHelper.patchAll(true);
			CraftHelper.patchAll();

			var typesConfig = Mod.loadConfig<TypesConfig>("types_config.json", Config.LoadOptions.ReadOnly | Config.LoadOptions.ProcessAttributes);
			GravTrapObjectsType.init(typesConfig);
		}
	}
}