using Harmony;

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
			
			var patch = AccessTools.Method(typeof(Vehicle_OnUpgradeModuleUse_Patch), "Postfix");
			HarmonyHelper.harmonyInstance.Patch(AccessTools.Method(typeof(Vehicle), "OnUpgradeModuleUse"), postfix: new HarmonyMethod(patch));
			HarmonyHelper.harmonyInstance.Patch(AccessTools.Method(typeof(SeaMoth), "OnUpgradeModuleUse"), postfix: new HarmonyMethod(patch));

			CraftHelper.patchAll();
		}
	}
}