using System;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common.Harmony;

namespace Common.Stasis
{
	static class Patches
	{
		public static readonly HarmonyHelper.LazyPatcher patcher = new();

		// stasis spheres will ignore vehicles
		[HarmonyTranspiler]
		[HarmonyHelper.Patch(typeof(StasisSphere), "Freeze")]
		[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.PatchOnce)]
		static IEnumerable<CodeInstruction> StasisSphere_Freeze_Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			static bool _isVehicle(Rigidbody target)
			{
				if (target.gameObject.GetComponent<Vehicle>())
					return true;
#if GAME_BZ
				if (target.gameObject.GetComponent<SeaTruckSegment>())
					return true;
#endif
				return false;
			}

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
}