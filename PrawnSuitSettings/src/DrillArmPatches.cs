using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

namespace PrawnSuitSettings
{
	// don't auto pickup resources after drilling
	[OptionalPatch, HarmonyPatch(typeof(Drillable), "ManagedUpdate")]
	static class Drillable_ManagedUpdate_Patch__ResourcesPickup
	{
		static bool Prepare() => !Main.config.autoPickupDrillableResources;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) =>
			CIHelper.ciRemove(cins, ci => ci.isOp(OpCodes.Ldloc_2), 0, 4);
	}

	// Toggleable drill arm
	[OptionalPatch, PatchClass]
	static class ToggleableDrillArmPatch
	{
		static bool prepare() => Main.config.toggleableDrillArm;

		public class ArmToggle: MonoBehaviour
		{
			bool usingArm = false;

			public bool toggleUsingArm() => usingArm = !usingArm;
			public bool setUsingArm(bool value) => usingArm = value;
			public bool isUsingArm() => usingArm;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Exosuit), "OnPilotModeBegin")]
		static void Exosuit_OnPilotModeBegin_Postfix(Exosuit __instance) =>
			__instance.GetComponentInChildren<ArmToggle>()?.setUsingArm(false);

		[HarmonyPrefix, HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.OnUseDown")]
		static void ExosuitDrillArm_OnUseDown_Prefix(ExosuitDrillArm __instance) =>
			__instance.gameObject.ensureComponent<ArmToggle>().toggleUsingArm();

		[HarmonyPrefix, HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.OnUseUp")]
		static bool ExosuitDrillArm_OnUseUp_Prefix(ExosuitDrillArm __instance) =>
			!__instance.gameObject.ensureComponent<ArmToggle>().isUsingArm();
	}
}