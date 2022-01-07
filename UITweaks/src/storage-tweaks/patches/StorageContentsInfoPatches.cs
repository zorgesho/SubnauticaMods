using System;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;

using Common;
using Common.Harmony;

namespace UITweaks.StorageTweaks
{
	partial class StorageContentsInfo
	{
		public static bool tweakEnabled => Main.config.storageTweaks.enabled && Main.config.storageTweaks.showContentsInfo;

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => tweakEnabled;

			[HarmonyTranspiler, HarmonyPatch(typeof(StorageContainer), "OnHandHover")]
			static IEnumerable<CodeInstruction> StorageContainer_OnHandHover_Transpiler(IEnumerable<CodeInstruction> cins)
			{
				static void _updateContentsInfo(StorageContainer instance) =>
					HandReticle.main.setText(textHandSubscript: instance.gameObject.ensureComponent<StorageContentsInfo>().getInfo());

				return cins.ciInsert(new CIHelper.MemberMatch(nameof(HandReticle.SetIcon)),
					OpCodes.Ldarg_0,
					CIHelper.emitCall<Action<StorageContainer>>(_updateContentsInfo));
			}
		}
	}
}