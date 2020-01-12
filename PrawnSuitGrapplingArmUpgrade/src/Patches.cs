using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using static Common.HarmonyHelper;

namespace PrawnSuitGrapplingArmUpgrade
{
	[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
	static class Exosuit_OnUpgradeModuleChange_Patch
	{
		static bool Prefix(Exosuit __instance, TechType techType)
		{
			if (techType == GrapplingArmUpgradeModule.TechType)
			{
				__instance.MarkArmsDirty();
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(Exosuit), "SpawnArm")]
	static class Exosuit_SpawnArm_Patch
	{
		static bool Prefix(Exosuit __instance, TechType techType, Transform parent, ref IExosuitArm __result)
		{
			if (techType != GrapplingArmUpgradeModule.TechType)
				return true;

			__result = __instance.SpawnArm(TechType.ExosuitGrapplingArmModule, parent);
			__result.GetGameObject().AddComponent<GrapplingArmUpgraded>();

			return false;
		}
	}

	// patching hook max distance, force & acceleration
	[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
	static class ExosuitGrapplingArm_FixedUpdate_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			var list = cins.ToList();

			for (int i = list.Count - 1; i >= 0; i--) // changing list in the process, so iterate it backwards
			{
				void tryChangeVal(float val, string configVar)
				{
					if (list[i].isLDC(val))
					{
						list.RemoveAt(i);
						list.InsertRange(i, _codeForChangeConstToConfigVar<float, GrapplingArmUpgraded>(val, configVar, ilg));
					}
				}

				tryChangeVal(35f, nameof(Main.config.hookMaxDistance));
				tryChangeVal(15f, nameof(Main.config.acceleration));
				tryChangeVal(400f, nameof(Main.config.force));
			}

			return list;
		}
	}

	// patching hook speed
	[HarmonyPatch(typeof(ExosuitGrapplingArm), "OnHit")]
	static class ExosuitGrapplingArm_OnHit_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			int ldc25 = 0;
			foreach (var ci in cins)
			{
				if (ci.isLDC(25f) && ++ldc25 == 2)
				{
					foreach (var i in _codeForChangeConstToConfigVar<float, GrapplingArmUpgraded>(25f, nameof(Main.config.hookSpeed), ilg))
						yield return i;

					continue;
				}

				yield return ci;
			}
		}
	}

	// patching cooldown
	[HarmonyPatch(typeof(ExosuitGrapplingArm), "IExosuitArm.OnUseDown")]
	static class ExosuitGrapplingArm_OnUseDown_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg) =>
			changeConstToConfigVar<float, GrapplingArmUpgraded>(cins, 2.0f, nameof(Main.config.armCooldown), ilg);
	}
}