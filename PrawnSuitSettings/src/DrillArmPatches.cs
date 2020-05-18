using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;

namespace PrawnSuitSettings
{
	// don't auto pickup resources after drilling
	[OptionalPatch]
	[HarmonyPatch(typeof(Drillable), "ManagedUpdate")]
	static class Drillable_ManagedUpdate_Patch__ResourcesPickup
	{
		static bool Prepare() => !Main.config.autoPickupDrillableResources;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) =>
			CIHelper.ciRemove(cins, ci => ci.isOp(OpCodes.Ldloc_2), 0, 4);
	}

	// Toggleable drill arm
	class PrawnSuitDrillArmToggle: MonoBehaviour
	{
		bool usingArm = false;

		public bool toggleUsingArm() => usingArm = !usingArm;
		public bool setUsingArm(bool value) => usingArm = value;
		public bool isUsingArm() => usingArm;
	}

	[OptionalPatch]
	[HarmonyPatch(typeof(Exosuit), "OnPilotModeBegin")]
	static class Exosuit_OnPilotModeBegin_Patch
	{
		static bool Prepare() => Main.config.toggleableDrillArm;

		static void Postfix(Exosuit __instance) =>
			__instance.GetComponentInChildren<PrawnSuitDrillArmToggle>()?.setUsingArm(false);
	}

	[OptionalPatch]
	[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.OnUseDown")]
	static class ExosuitDrillArm_OnUseDown_Patch
	{
		static bool Prepare() => Main.config.toggleableDrillArm;

		static void Prefix(ExosuitDrillArm __instance) =>
			__instance.gameObject.ensureComponent<PrawnSuitDrillArmToggle>().toggleUsingArm();
	}

	[OptionalPatch]
	[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.OnUseUp")]
	static class ExosuitDrillArm_OnUseUp_Patch
	{
		static bool Prepare() => Main.config.toggleableDrillArm;

		static bool Prefix(ExosuitDrillArm __instance) =>
			!__instance.gameObject.ensureComponent<PrawnSuitDrillArmToggle>().isUsingArm();
	}
}