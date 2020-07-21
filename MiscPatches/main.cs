using Common;
using Common.Harmony;

namespace MiscPatches
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			MiscStuff.init();

			PersistentConsoleCommands_2.register<ConsoleCommands>();
		}
	}
}