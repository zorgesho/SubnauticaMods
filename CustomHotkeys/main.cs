using Common;
using Common.Harmony;
using Common.Configuration;

namespace CustomHotkeys
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		internal const string hotkeyConfigName = "hotkeys.json";
		internal static readonly HKConfig hkConfig = Mod.loadConfig<HKConfig>(hotkeyConfigName, Config.LoadOptions.ProcessAttributes);

		public static void patch()
		{
			HarmonyHelper.patchAll(true);

			if (config.addConsoleCommands)
				PersistentConsoleCommands.createGameObject<ConsoleCommands>();
		}
	}
}