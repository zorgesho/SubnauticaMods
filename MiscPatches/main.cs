using Common;
using Common.Configuration;

namespace MiscPatches
{
	public static class Main
	{
		internal static readonly ModConfig config =
			Config.tryLoad<ModConfig>(loadOptions: Config.LoadOptions.ForcedLoad | Config.LoadOptions.ProcessAttributes);

		public static void patch()
		{
			HarmonyHelper.patchAll();

			ConsoleCommands.init();

			MiscStuff.init();
		}
	}
}