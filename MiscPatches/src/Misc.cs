using System.Text;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.Events;

using Common;
using Common.Harmony;
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
			{
				GameObject prefab = CraftData.GetPrefabForTechType(TechType.BlueCluster);
				deathEffect = prefab.GetComponent<LiveMixin>().data.deathEffect;
			}

			// can't change it just once, stingers use three LiveMixinData (short, middle, long)
			liveMixin.data.destroyOnDeath = true;
			liveMixin.data.explodeOnDestroy = false;
			liveMixin.data.deathEffect = deathEffect;
			liveMixin.data.maxHealth = maxHealth;

			liveMixin.health = liveMixin.data.maxHealth;
		}
	}

	// For adding propulsion/repulsion cannon immunity to some objects
	// for now: <BrainCoral> <Drillable>
	static class PropRepCannonImmunity
	{
		class ImmuneToPropRepCannon: MonoBehaviour {}

		public static bool isObjectImmune(GameObject go)
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

		[HarmonyPatch(typeof(PropulsionCannon), "ValidateNewObject")]
		static class PropulsionCannon_ValidateNewObject_Patch
		{
			static bool Prepare() => Main.config.additionalPropRepImmunity;
			static bool Prefix(GameObject go, ref bool __result) => __result = !isObjectImmune(go);
		}

		[HarmonyPatch(typeof(RepulsionCannon), "OnToolUseAnim")]
		static class RepulsionCannon_OnToolUseAnim_Patch
		{
			static bool Prepare() => Main.config.additionalPropRepImmunity;

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
			{
				var list = cins.ToList();

				int indexForInject = list.FindIndex(ci => ci.isOp(OpCodes.Brfalse)) + 5;
				int indexForJump   = list.FindIndex(indexForInject, ci => ci.isOp(OpCodes.Brfalse));

				Common.Debug.assert(indexForInject >= 5 && indexForJump != -1);

				CIHelper.ciInsert(list, indexForInject, new List<CodeInstruction>()
				{
					new CodeInstruction(OpCodes.Ldloc_S, 11),
					new CodeInstruction(OpCodes.Call, typeof(PropRepCannonImmunity).method(nameof(PropRepCannonImmunity.isObjectImmune))),
					new CodeInstruction(OpCodes.Brtrue, list[indexForJump].operand)
				});

				return list;
			}
		}
	}


	static class ChangeChargersSpeed
	{
		// chargers speed is not linked to battery capacity
		[HarmonyPatch(typeof(Charger), "Update")]
		static class Charger_Update_Patch
		{
			static bool Prepare() => Main.config.changeChargersSpeed && Main.config.chargersAbsoluteSpeed;

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
			{
				FieldInfo chargeSpeed = typeof(Charger).field("chargeSpeed");
				return CIHelper.ciRemove(cins, ci => ci.isOp(OpCodes.Ldfld, chargeSpeed), +2, 2); // remove "*capacity"
			}
		}

		// BatteryCharger speed
		[HarmonyPatch(typeof(BatteryCharger), "Initialize")]
		static class BatteryCharger_Initialize_Patch
		{
			static bool Prepare() => Main.config.changeChargersSpeed;

			static void Postfix(Charger __instance) =>
				__instance.chargeSpeed = Main.config.batteryChargerSpeed * (Main.config.chargersAbsoluteSpeed? 100f: 1f);
		}

		// PowerCellCharger speed
		[HarmonyPatch(typeof(PowerCellCharger), "Initialize")]
		static class PowerCellCharger_Initialize_Patch
		{
			static bool Prepare() => Main.config.changeChargersSpeed;

			static void Postfix(Charger __instance) =>
				__instance.chargeSpeed = Main.config.powerCellChargerSpeed * (Main.config.chargersAbsoluteSpeed? 200f: 1f);
		}
	}


	static class MiscStuff
	{
		public static void init()
		{
			CraftData.useEatSound.Add(TechType.Coffee, "event:/player/drink");
		}
	}
}