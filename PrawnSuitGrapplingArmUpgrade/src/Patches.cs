using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common.Harmony;

namespace PrawnSuitGrapplingArmUpgrade
{
	using static CIHelper;
	using CIEnumerable = IEnumerable<CodeInstruction>;

	[HarmonyPatch(typeof(ExosuitGrapplingArm), "Start")]
	static class ExosuitGrapplingArm_Start_Patch
	{
		static void Postfix(ExosuitGrapplingArm __instance)
		{
			if (!__instance.GetComponent<GrapplingArmUpgraded>())
				return;

			__instance.hook.GetComponent<SphereCollider>().radius = 0.25f; // from 0.5f
			__instance.hook.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		}
	}

	[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
	static class Exosuit_OnUpgradeModuleChange_Patch
	{
		static bool Prefix(Exosuit __instance, TechType techType)
		{
			if (techType != GrapplingArmUpgradeModule.TechType)
				return true;

			__instance.MarkArmsDirty();
			return false;
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

	// fix for vanilla bug (left arm's hook orientation in the attached state)
	[HarmonyPatch(typeof(GrapplingHook), "OnCollisionEnter")]
	static class GrapplingHook_OnCollisionEnter_Patch
	{
		static float getAngle(GrapplingHook hook) => Math.Sign(hook.transform.localScale.x) * -90f;

		static CIEnumerable Transpiler(CIEnumerable cins) =>
			cins.ciReplace(ci => ci.isLDC(90f), OpCodes.Ldarg_0, emitCall<Func<GrapplingHook, float>>(getAngle));
	}

	// patching hook max distance, force & acceleration
	[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
	static class ExosuitGrapplingArm_FixedUpdate_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins, ILGenerator ilg)
		{
			var list = cins.ToList();

			void _changeVal(float val, string cfgVar) => constToCfgVar<float, GrapplingArmUpgraded>(list, val, cfgVar, ilg);

			_changeVal(35f,  nameof(Main.config.hookMaxDistance));
			_changeVal(15f,  nameof(Main.config.acceleration));
			_changeVal(400f, nameof(Main.config.force));

			return list;
		}
	}

	// patching hook speed
	[HarmonyPatch(typeof(ExosuitGrapplingArm), "OnHit")]
	static class ExosuitGrapplingArm_OnHit_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins, ILGenerator ilg)
		{
			int ldc25 = 0;
			return cins.ciReplace(ci => ci.isLDC(25f) && ++ldc25 == 2,
				_codeForCfgVar<float, GrapplingArmUpgraded>(25f, nameof(Main.config.hookSpeed), ilg));
		}
	}

	// patching cooldown
	[HarmonyPatch(typeof(ExosuitGrapplingArm), "IExosuitArm.OnUseDown")]
	static class ExosuitGrapplingArm_OnUseDown_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins, ILGenerator ilg) =>
			constToCfgVar<float, GrapplingArmUpgraded>(cins, 2.0f, nameof(Main.config.armCooldown), ilg);
	}
}