using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;

namespace RemoteTorpedoDetonator
{
	[HarmonyHelper.OptionalPatch]
	[HarmonyPatch(typeof(SeamothTorpedo), "Awake")]
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

	[HarmonyHelper.PatchClass]
	static class Vehicle_OnUpgradeModuleUse_Patch
	{
		static void postfix(Vehicle vehicle, TechType techType, int slotID)
		{
			if (techType == TorpedoDetonatorModule.TechType)
				vehicle.gameObject.GetComponent<TorpedoDetonatorControl>()?.detonateTorpedoes();

			if (techType == TechType.SeamothTorpedoModule && vehicle.quickSlotCooldown[slotID] > Main.config.torpedoCooldown)
				vehicle.quickSlotCooldown[slotID] = Main.config.torpedoCooldown;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleUse")]
		static void vehiclePatch(Vehicle __instance, TechType techType, int slotID) => postfix(__instance, techType, slotID);

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SeaMoth), "OnUpgradeModuleUse")]
		static void seamothPatch(Vehicle __instance, TechType techType, int slotID) => postfix(__instance, techType, slotID);
	}

	// infinite torpedoes cheat
	[HarmonyHelper.OptionalPatch]
	[HarmonyPatch(typeof(Vehicle), "TorpedoShot")]
	static class Vehicle_TorpedoShot_Patch
	{
		static bool Prepare() => Main.config.cheatInfiniteTorpedoes;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) =>
			HarmonyHelper.ciRemove(cins, ci => ci.isOp(OpCodes.Ldarg_0), +0, 5); // removing "container.DestroyItem(torpedoType.techType)" check
	}
}