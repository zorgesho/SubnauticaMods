using Common;
using Common.Harmony;
using Common.Crafting;
using Common.Configuration;

namespace GravTrapImproved
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();
		internal static readonly TypesConfig typesConfig = Mod.loadConfig<TypesConfig>("types_config.json", Config.LoadOptions.ReadOnly | Config.LoadOptions.ProcessAttributes);

		public static void patch()
		{
			LanguageHelper.init();
			PersistentConsoleCommands.register<ConsoleCommands>();

			HarmonyHelper.patchAll(true);
			CraftHelper.patchAll();

			GravTrapObjectsType.init(typesConfig);
		}
	}
}