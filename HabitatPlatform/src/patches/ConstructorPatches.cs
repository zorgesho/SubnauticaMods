using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common.Harmony;
using Common.Reflection;

namespace HabitatPlatform
{
	// same spawn point as for RocketBase
	[HarmonyPatch(typeof(Constructor), "GetItemSpawnPoint")]
	static class Constructor_GetItemSpawnPoint_Patch
	{
		static bool Prefix(Constructor __instance, TechType techType, ref Transform __result)
		{
			if (techType != HabitatPlatform.TechType)
				return true;

			__result = __instance.GetItemSpawnPoint(TechType.RocketBase);
			return false;
		}
	}

	// set construction duration for habitat platform same as for rocket platform (25 sec)
	[HarmonyPatch(typeof(ConstructorInput), "Craft")]
	static class ConstructorInput_Craft_Patch
	{
#if DEBUG
		static bool Prepare() => !Main.config.dbgFastPlatformBuild;
#endif
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			var list = cins.ToList();

			int index = list.FindIndex(ci => ci.isLDC((int)TechType.RocketBase));

			Label labelAssign = ilg.DefineLabel();
			Label labelExit = list[index + 1].operand.cast<Label>();

			list[index + 1] = new CodeInstruction(OpCodes.Beq_S, labelAssign);

			CIHelper.ciInsert(list, index + 2,
				OpCodes.Ldarg_1,
				OpCodes.Call, typeof(HabitatPlatform).method("get_TechType"),
				OpCodes.Bne_Un_S, labelExit);

			list[list.FindIndex(index, ci => ci.isLDC(25f))].labels.Add(labelAssign);

			return list;
		}
	}
}