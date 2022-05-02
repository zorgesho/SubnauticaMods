using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;

using Common.Harmony;

namespace UITweaks.StorageTweaks
{
	[OptionalPatch, PatchClass]
	static class CompatibilityPatches
	{
		static bool prepare()
		{
			var st = Main.config.storageTweaks;
			return st.enabled && (st.allInOneActions || st.showContentsInfo);
		}

		[HarmonyTranspiler, HarmonyHelper.Patch(HarmonyHelper.PatchOptions.CanBeAbsent)]
		[HarmonyHelper.Patch("Tweaks_Fixes.Storage_Patch+StorageContainer_Patch, Tweaks and Fixes", "OnHandHoverPrefix")]
		static IEnumerable<CodeInstruction> patchDisabler(IEnumerable<CodeInstruction> _) =>
			CIHelper.toCIList(OpCodes.Ldc_I4_1, OpCodes.Ret);
	}
}