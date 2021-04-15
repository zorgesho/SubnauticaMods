using System.Linq;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(Player), "Awake")]
	static class Test_Patch
	{
		static bool Prefix(Player __instance)
		{
			return true;
		}

		static void Postfix(Player __instance)
		{
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();
			return list;
		}
	}


	[PatchClass]
	static class Patches
	{
		[HarmonyPrefix, HarmonyPatch(typeof(Player), "Awake")]
		static bool Test_Prefix(Player __instance)
		{
			return true;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Player), "Awake")]
		static void Test_Postfix(Player __instance)
		{
		}

		[HarmonyTranspiler, HarmonyPatch(typeof(Player), "Awake")]
		static IEnumerable<CodeInstruction> Test_Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();
			return list;
		}
	}
}