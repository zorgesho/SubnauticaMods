using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;

using Common;
using Common.Harmony;

namespace RemoteTorpedoDetonator
{
	[OptionalPatch, HarmonyPatch(typeof(SeamothTorpedo), "Awake")]
	static class SeamothTorpedo_Awake_Patch
	{
		static bool Prepare() => Main.config.torpedoSpeed != 10f || !Main.config.homingTorpedoes; // non-default values, need to patch

		static void Postfix(SeamothTorpedo __instance)
		{
			__instance.homingTorpedo = Main.config.homingTorpedoes;
			__instance.speed = Main.config.torpedoSpeed;
		}
	}

	[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleChange")]
	static class Vehicle_OnUpgradeModuleChange_Patch
	{
		static void Postfix(Vehicle __instance, TechType techType)
		{
			if (techType == TorpedoDetonatorModule.TechType)
				__instance.gameObject.ensureComponent<TorpedoDetonatorControl>().checkEnabled();
		}
	}

	[PatchClass]
	static class Vehicle_OnUpgradeModuleUse_Patch
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(SeaMoth), "OnUpgradeModuleUse")]
		[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleUse")]
		static void OnUpgradeModuleUse_Postfix(Vehicle __instance, TechType techType, int slotID)
		{
			if (techType == TorpedoDetonatorModule.TechType)
				__instance.gameObject.GetComponent<TorpedoDetonatorControl>()?.detonateTorpedoes();

			if (techType == TechType.SeamothTorpedoModule && __instance.quickSlotCooldown[slotID] > Main.config.torpedoCooldown)
				__instance.quickSlotCooldown[slotID] = Main.config.torpedoCooldown;
		}
	}

	// infinite torpedoes cheat
	[OptionalPatch, HarmonyPatch(typeof(Vehicle), "TorpedoShot")]
	static class Vehicle_TorpedoShot_Patch
	{
		static bool Prepare() => Main.config.cheatInfiniteTorpedoes;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) =>
			CIHelper.ciRemove(cins, ci => ci.isOp(OpCodes.Ldarg_0), +0, 5); // removing "container.DestroyItem(torpedoType.techType)" check
	}
}