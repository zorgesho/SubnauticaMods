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

			HarmonyHelper.patchAll();
			CraftHelper.patchAll();

			if (config.mk2Enabled)
				HarmonyHelper.patch(typeof(GravTrapMK2Patches));

			GravTrapObjectsType.init(Mod.loadConfig<TypesConfig>("types_config.json", Config.LoadOptions.ReadOnly));
		}
	}
}