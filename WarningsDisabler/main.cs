using QModManager.API.ModLoading;

using Common;
using Common.Harmony;

namespace WarningsDisabler
{
	[QModCore]
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		[QModPrePatch]
		public static void patch()
		{
			HarmonyHelper.patchAll(true);
			LanguageHelper.init();
		}
	}
}