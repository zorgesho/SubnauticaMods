using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using Harmony;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	// don't auto pickup resources after drilling
	[HarmonyHelper.OptionalPatch(typeof(Drillable), "ManagedUpdate")]
	static class DrillableResourcesPickup
	{
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction() => refresh();
		}

		public static void refresh() => HarmonyHelper.setPatchEnabled(!Main.config.autoPickupDrillableResources, typeof(DrillableResourcesPickup));

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = new List<CodeInstruction>(cins);

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

	// Toggleable drill arm
	class PrawnSuitDrillArmToggle: MonoBehaviour
	{
		bool usingArm = false;

		public bool toggleUsingArm() => usingArm = !usingArm;
		public bool setUsingArm(bool value) => usingArm = value;
		public bool isUsingArm() => usingArm;
	}

	[HarmonyPatch(typeof(Exosuit), "OnPilotModeBegin")]
	static class Exosuit_OnPilotModeBegin_Patch
	{
		static void Postfix(Exosuit __instance)
		{
			if (Main.config.toggleableDrillArm)
				__instance.GetComponentInChildren<PrawnSuitDrillArmToggle>()?.setUsingArm(false);
		}
	}

	[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.OnUseDown")]
	static class ExosuitDrillArm_OnUseDown_Patch
	{
		static void Prefix(ExosuitDrillArm __instance)
		{
			if (Main.config.toggleableDrillArm)
				__instance.gameObject.getOrAddComponent<PrawnSuitDrillArmToggle>().toggleUsingArm();
		}
	}

	[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.OnUseUp")]
	static class ExosuitDrillArm_OnUseUp_Patch
	{
		static bool Prefix(ExosuitDrillArm __instance) =>
			!(Main.config.toggleableDrillArm && __instance.gameObject.getOrAddComponent<PrawnSuitDrillArmToggle>().isUsingArm());
	}
}