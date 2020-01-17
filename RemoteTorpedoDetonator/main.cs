using Common;
using Common.Crafting;
using Common.Configuration;

namespace RemoteTorpedoDetonator
{
	public static class Main
	{
		internal static ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			LanguageHelper.init();

			var patch = typeof(Vehicle_OnUpgradeModuleUse_Patch).method("Postfix");
			HarmonyHelper.patch(typeof(Vehicle).method("OnUpgradeModuleUse"), postfix: patch);
			HarmonyHelper.patch(typeof(SeaMoth).method("OnUpgradeModuleUse"), postfix: patch);

			CraftHelper.patchAll();
		}
	}
}