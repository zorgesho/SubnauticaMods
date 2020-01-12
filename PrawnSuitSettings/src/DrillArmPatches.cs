using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

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
			var list = cins.ToList();

			int index = list.FindIndex(ci => ci.isOp(OpCodes.Ldloc_2));

			if (index > 0)
				list.RemoveRange(index, 4);

			return list;
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