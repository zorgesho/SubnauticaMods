using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common.Harmony;
using Common.Configuration;

namespace GravTrapImproved
{
	static class GravTrapMK2Patches
	{
		public static void updateRange(Gravsphere gravsphere)
		{
			if (!gravsphere.GetComponent<GravTrapMK2.Tag>())
				return;

			if (gravsphere.gameObject.GetComponents<SphereCollider>()?.FirstOrDefault(s => s.radius > 10) is SphereCollider sphere)
				sphere.radius = Main.config.mk2Range;
		}

		public class UpdateRanges: Config.Field.IAction
		{
			public void action() => Object.FindObjectsOfType<Gravsphere>().ForEach(sphere => updateRange(sphere));
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(Gravsphere), "Start")] // patching work range
		static void patchMaxRadius(Gravsphere __instance)
		{
			updateRange(__instance);
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Gravsphere), "OnTriggerEnter")] // patching max count of attracted objects
		static IEnumerable<CodeInstruction> patchObjectMaxCount(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			return CIHelper.constToCfgVar<sbyte, GravTrapMK2.Tag>(cins, 12, nameof(Main.config.mk2MaxObjects), ilg);
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Gravsphere), "ApplyGravitation")] // patching max force applied to attracted objects
		static IEnumerable<CodeInstruction> patchMaxForce(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			return CIHelper.constToCfgVar<float, GravTrapMK2.Tag>(cins, 15f, nameof(Main.config.mk2MaxForce), ilg);
		}


		static readonly string gravtrap = nameof(GravTrapMK2).ToLower();

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuickSlots), "SetAnimationState")] // using animation from vanilla gravtrap
		static bool patchAnimation(QuickSlots __instance, string toolName)
		{
			if (toolName != gravtrap)
				return true;

			__instance.SetAnimationState("gravsphere");
			return false;
		}
	}
}