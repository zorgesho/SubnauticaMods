using System;
using Harmony;

namespace PrawnSuitSettings
{
	// get access to prawn suit stuff when docked in moonpool
	[HarmonyPatch(typeof(Exosuit), "UpdateColliders")]
	static class Exosuit_UpdateColliders_Patch
	{
		static bool Prepare() => Main.config.accessToPrawnSuitPartsWhenDocked;

		static readonly string[] enabledColliders = {"Storage", "UpgradeConsole", "BatteryLeft", "BatteryRight"};

		static void Postfix(Exosuit __instance)
		{
			for (int i = 0; i < __instance.disableDockedColliders.Length; i++)
			{
				if (Array.IndexOf(enabledColliders, __instance.disableDockedColliders[i].name) != -1)
					__instance.disableDockedColliders[i].enabled = true;
			}
		}
	}
}