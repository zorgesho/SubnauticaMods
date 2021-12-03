using System;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common.Harmony;

namespace StasisModule
{
	[PatchClass]
	static class Patches
	{
		// stasis spheres will ignore vehicles
		[HarmonyTranspiler, HarmonyPatch(typeof(StasisSphere), "Freeze")]
		static IEnumerable<CodeInstruction> StasisSphere_Freeze_Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			static bool _isVehicle(Rigidbody target) => target.gameObject.GetComponent<Vehicle>();

			var label = ilg.DefineLabel();

			return cins.ciInsert(ci => ci.isOp(OpCodes.Ret), // right after null check
				OpCodes.Ldarg_2,
				OpCodes.Ldind_Ref,
				CIHelper.emitCall<Func<Rigidbody, bool>>(_isVehicle),
				OpCodes.Brfalse, label,
				OpCodes.Ldc_I4_0,
				OpCodes.Ret,
				new CodeInstruction(OpCodes.Nop) { labels = { label } });
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
			if (techType != StasisModule.TechType || !__instance.HasEnoughEnergy(Main.config.energyCost))
				return;

			__instance.ConsumeEnergy(Main.config.energyCost);
			__instance.quickSlotTimeUsed[slotID] = Time.time;
			__instance.quickSlotCooldown[slotID] = Main.config.cooldown;

			var go = new GameObject("stasis", typeof(StasisModule.StasisExplosion));
			go.transform.position = __instance.transform.position;
		}
	}
}