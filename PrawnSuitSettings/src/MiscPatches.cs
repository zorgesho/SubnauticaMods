using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Configuration;

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

	
	// don't auto pickup resources after drilling
	[HarmonyHelper.OptionalPatch(typeof(Drillable), "ManagedUpdate")]
	static class DrillableResourcesPickup
	{
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction() => refresh();
		}
		
		public static void refresh() => HarmonyHelper.setPatchEnabled(!Main.config.autoPickupDrillableResources, typeof(DrillableResourcesPickup));
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var list = new List<CodeInstruction>(instructions);

			for (int i = 0; i < list.Count - 1; i++)
			{
				if (list[i].opcode == OpCodes.Ldloc_1 && list[i + 1].opcode == OpCodes.Ldnull)
				{
					list.RemoveRange(i, 4);
					break;
				}
			}

			return list.AsEnumerable();
		}
	}
}