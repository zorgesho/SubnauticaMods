using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace StasisTorpedo
{
	[PatchClass]
	static class Patches
	{
		[HarmonyPostfix, HarmonyPatch(typeof(Vehicle), "Awake")]
		static void Vehicle_Awake_Postfix(Vehicle __instance)
		{																																		$"Vehicle.Awake: {__instance.gameObject.name}".logDbg();
			if (StasisTorpedo.torpedoType == null)
				StasisTorpedo.initPrefab(__instance.torpedoTypes.FirstOrDefault(type => type.techType == TechType.GasTorpedo)?.prefab);

			__instance.torpedoTypes = __instance.torpedoTypes.append(new[] { StasisTorpedo.torpedoType });
		}

		// to fix vanilla bug with torpedoes double explosions
		[HarmonyPrefix, HarmonyPatch(typeof(SeamothTorpedo), "OnEnergyDepleted")]
		static bool SeamothTorpedo_OnEnergyDepleted_Prefix(SeamothTorpedo __instance)
		{
			return __instance._active;
		}

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
#if GAME_SN
		// allow to put stasis torpedoes to the seamoth torpedo storage
		[HarmonyTranspiler, HarmonyPatch(typeof(SeaMoth), "OpenTorpedoStorage")]
		static IEnumerable<CodeInstruction> SeaMoth_OpenTorpedoStorage_Transpiler(IEnumerable<CodeInstruction> cins)
		{
			return cins.ciInsert(new CIHelper.MemberMatch(nameof(SeaMoth.GetStorageInSlot)),
				OpCodes.Dup,
				ensureTorpedoAllowed());
		}
#endif
		// allow to put stasis torpedoes to the prawn suit torpedo arm
		[HarmonyTranspiler, HarmonyPatch(typeof(ExosuitTorpedoArm), "OpenTorpedoStorageExternal")]
		static IEnumerable<CodeInstruction> ExosuitTorpedoArm_OpenTorpedoStorageExternal_Transpiler(IEnumerable<CodeInstruction> cins)
		{
			return cins.ciInsert(ci => ci.isOp(OpCodes.Ret),
				OpCodes.Ldarg_0,
				OpCodes.Ldfld, typeof(ExosuitTorpedoArm).field("container"),
				ensureTorpedoAllowed());
		}

		static CodeInstruction ensureTorpedoAllowed()
		{
			static void _ensureTorpedoAllowed(ItemsContainer container) =>
				container?.allowedTech.Add(StasisTorpedo.TechType);

			return CIHelper.emitCall<Action<ItemsContainer>>(_ensureTorpedoAllowed);
		}
	}
}