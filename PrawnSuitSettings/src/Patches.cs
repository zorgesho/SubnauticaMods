using System;
using Harmony;

namespace PrawnSuitSettings
{
	// get access to prawn suit stuff when docked in moonpool
	[HarmonyPatch(typeof(Exosuit), "UpdateColliders")]
	static class Exosuit_UpdateColliders_Patch
	{
		static readonly string[] enabledColliders = {"Storage", "UpgradeConsole", "BatteryLeft", "BatteryRight"};

		static void Postfix(Exosuit __instance)
		{
			if (!Main.config.accessToPrawnSuitPartsWhenDocked)
				return;

			for (int i = 0; i < __instance.disableDockedColliders.Length; i++)
			{
				if (Array.IndexOf(enabledColliders, __instance.disableDockedColliders[i].name) != -1)
					__instance.disableDockedColliders[i].enabled = true;
			}
		}
	}


	[HarmonyPatch(typeof(PropulsionCannon), "UpdateActive")]
	static class PropulsionCannonExosuit_UpdateActive_Patch
	{
		static void Postfix(PropulsionCannon __instance)
		{
			if (Main.config.passivePropulsionCannon && Player.main.GetVehicle() != null)
				__instance.animator.SetBool("cangrab_propulsioncannon", false);
		}
	}
}