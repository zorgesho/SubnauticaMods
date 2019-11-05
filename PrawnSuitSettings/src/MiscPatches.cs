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
			if (!Main.config.fullAccessToPrawnSuitWhileDocked)
				return;

			for (int i = 0; i < __instance.disableDockedColliders.Length; i++)
			{
				if (Array.IndexOf(enabledColliders, __instance.disableDockedColliders[i].name) != -1)
					__instance.disableDockedColliders[i].enabled = true;
			}
		}
	}


	// don't play propulsion cannon arm 'ready' animation when pointed on pickable object
	[HarmonyPatch(typeof(PropulsionCannon), "UpdateActive")]
	static class PropulsionCannon_UpdateActive_Patch
	{
		static void Postfix(PropulsionCannon __instance)
		{
			if (!Main.config.readyAnimationForPropulsionCannon && Player.main.GetVehicle() != null)
				__instance.animator.SetBool("cangrab_propulsioncannon", __instance.grabbedObject != null);
		}
	}
}