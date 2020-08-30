using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.Events;

using Common;
using Common.Harmony;
using Common.Crafting;
using Common.Reflection;

namespace MiscPatches
{
	//Adjusting whirlpool torpedo punch force
	[HarmonyPatch(typeof(SeamothTorpedoWhirlpool), "Awake")]
	static class SeamothTorpedoWhirlpool_Awake_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(SeamothTorpedoWhirlpool __instance) => __instance.punchForce = Main.config.torpedoPunchForce;
	}

	// change flares burn time and intensity
	[HarmonyPatch(typeof(Flare), "Awake")]
	static class Flare_Awake_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(Flare __instance)
		{
			if (__instance.energyLeft == 1800)
				__instance.energyLeft = Main.config.flareBurnTime;

			__instance.originalIntensity = Main.config.flareIntensity;
		}
	}

	// flare in inventory shows whether it is lighted
	[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
	static class TooltipFactory_ItemCommons_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
		{
			if (techType == TechType.Flare)
			{
				var flare = obj.GetComponent<Flare>();
				if (flare.hasBeenThrown)
					TooltipFactory.WriteDescription(sb, "[lighted]");
			}
		}
	}

	// Stop dead creatures twitching animations (stop any animations, to be clear)
	[HarmonyPatch(typeof(CreatureDeath), "OnKill")]
	static class CreatureDeath_OnKill_Patch
	{
		const float timeToStopAnimator = 5f;

		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(CreatureDeath __instance) =>
			__instance.gameObject.callAfterDelay(timeToStopAnimator, new UnityAction(() =>
			{
				if (__instance.gameObject.GetComponentInChildren<Animator>() is Animator animator)
					animator.enabled = false;
			}));
	}

	// we can kill HangingStingers now
	[HarmonyPatch(typeof(HangingStinger), "Start")]
	static class HangingStinger_Start_Patch
	{
		static GameObject deathEffect = null;
		const float maxHealth = 10f;

		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(HangingStinger __instance)
		{
			LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();

			if (!deathEffect)
				UWE.CoroutineHost.StartCoroutine(_deathEffect());

			// can't change it just once, stingers use three LiveMixinData (short, middle, long)
			liveMixin.data.destroyOnDeath = true;
			liveMixin.data.explodeOnDestroy = false;
			liveMixin.data.deathEffect = deathEffect;
			liveMixin.data.maxHealth = maxHealth;

			liveMixin.health = liveMixin.data.maxHealth;

			static IEnumerator _deathEffect() // multiple ?
			{
				var task = PrefabUtils.getPrefabAsync(TechType.BlueCluster);
				yield return task;
				deathEffect = task.GetResult().GetComponent<LiveMixin>().data.deathEffect;
			}
		}
	}

	// disable first use animations for tools and escape pod hatch cinematics
	[OptionalPatch, PatchClass]
	static class FirstAnimationsPatch
	{
		static bool prepare() => !Main.config.firstAnimations;

		[HarmonyPrefix, HarmonyPatch(typeof(Player), "AddUsedTool")]
		static bool Player_AddUsedTool_Prefix(ref bool __result) => __result = false;

		[HarmonyPrefix, HarmonyPatch(typeof(EscapePod), "Awake")]
		static void EscapePod_Awake_Prefix(EscapePod __instance) => __instance.bottomHatchUsed = __instance.topHatchUsed = true;
	}

	// For adding propulsion/repulsion cannon immunity to some objects
	// for now: <BrainCoral> <Drillable>
	[PatchClass]
	static class PropRepCannonImmunity
	{
		class ImmuneToPropRepCannon: MonoBehaviour {}

		static bool prepare() => Main.config.additionalPropRepImmunity;

		static bool isObjectImmune(GameObject go)
		{
			if (!go || go.GetComponent<ImmuneToPropRepCannon>())
				return true;

			if (go.GetComponent<BrainCoral>() || go.GetComponent<Drillable>()) // maybe I'll add some more
			{
				go.AddComponent<ImmuneToPropRepCannon>();
				return true;
			}

			return false;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(PropulsionCannon), "ValidateNewObject")]
		static bool PropulsionCannon_ValidateNewObject_Prefix(GameObject go, ref bool __result) =>
			__result = !isObjectImmune(go);

		[HarmonyTranspiler, HarmonyPatch(typeof(RepulsionCannon), "OnToolUseAnim")]
		static IEnumerable<CodeInstruction> RepulsionCannon_OnToolUseAnim_Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();

			int[] i = list.ciFindIndexes(ci => ci.isOp(OpCodes.Brfalse),
										 ci => ci.isOpLoc(OpCodes.Ldloc_S, 11), // Rigidbody component = gameObject.GetComponent<Rigidbody>();
										 ci => ci.isOp(OpCodes.Brfalse));
			return i == null? cins:
				list.ciInsert(i[1],
					OpCodes.Ldloc_S, 11,
					CIHelper.emitCall<Func<GameObject, bool>>(isObjectImmune),
					OpCodes.Brtrue, list[i[2]].operand);
		}
	}

	[PatchClass]
	static class ChangeChargersSpeed
	{
		static bool prepare() => Main.config.changeChargersSpeed;

		[HarmonyPostfix, HarmonyPatch(typeof(BatteryCharger), "Initialize")] // BatteryCharger speed
		static void BatteryCharger_Initialize_Postfix(Charger __instance)
		{
			__instance.chargeSpeed = Main.config.batteryChargerSpeed * (Main.config.chargersAbsoluteSpeed? 100f: 1f);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(PowerCellCharger), "Initialize")] // PowerCellCharger speed
		static void PowerCellCharger_Initialize_Postfix(Charger __instance)
		{
			__instance.chargeSpeed = Main.config.powerCellChargerSpeed * (Main.config.chargersAbsoluteSpeed? 200f: 1f);
		}

		// chargers speed is not linked to battery capacity
		[HarmonyPatch(typeof(Charger), "Update")]
		static class Charger_Update_Patch
		{
			static bool Prepare() => prepare() && Main.config.chargersAbsoluteSpeed;

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
			{
				FieldInfo chargeSpeed = typeof(Charger).field("chargeSpeed");
				return CIHelper.ciRemove(cins, ci => ci.isOp(OpCodes.Ldfld, chargeSpeed), +2, 2); // remove "*capacity"
			}
		}
	}

	[OptionalPatch, HarmonyPatch(typeof(WaterscapeVolume), "RenderImage")] // from ExtraOptions mod
	static class FogFixPatch
	{
		static bool Prepare() => Main.config.fixFog;
		static void Prefix(ref bool cameraInside) => cameraInside = false;
	}

	static class MiscStuff
	{
		public static void init()
		{
			CraftData.useEatSound.Add(TechType.Coffee, "event:/player/drink");
		}
	}
}