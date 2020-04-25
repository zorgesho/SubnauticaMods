using Common;
using Common.Crafting;

namespace MiscPrototypes
{
	public static partial class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		static partial void initTestConfig();

		public static void patch()
		{
			initTestConfig();

			HarmonyHelper.patchAll(true);
			LanguageHelper.init();
			CraftHelper.patchAll();
		}
	}
}