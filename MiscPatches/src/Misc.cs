using System.Text;

using Harmony;
using UnityEngine;
using UnityEngine.Events;

using Common;

namespace MiscPatches
{
	//Adjusting whirlpool torpedo punch force
	[HarmonyPatch(typeof(SeamothTorpedoWhirlpool), "Awake")]
	static class SeamothTorpedoWhirlpool_Awake_Patch
	{
		static void Postfix(SeamothTorpedoWhirlpool __instance) => __instance.punchForce = Main.config.torpedoPunchForce;
	}

	// change flares burn time and intensity
	[HarmonyPatch(typeof(Flare), "Awake")]
	static class Flare_Awake_Patch
	{
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

		static void Postfix(CreatureDeath __instance) =>
			__instance.gameObject.callAfterDelay(timeToStopAnimator, new UnityAction(() =>
			{
				if (__instance.gameObject.GetComponentInChildren<Animator>() is Animator animator)
					animator.enabled = false;
			}));
	}

	
	static class MiscStuff
	{
		public static void init()
		{
			CraftData.useEatSound.Add(TechType.Coffee, "event:/player/drink");
		}
	}

	// fixing known eggs that appear as unknown
	// most of the eggs somehow dont subscribe to KnownTech event and remain unknown
	// UPDATE: it seems to be fixed in Nov-06 update
	//[HarmonyPatch(typeof(Pickupable), "OnHandHover")]
	//static class Pickupable_OnHandHover_Patch
	//{
	//	static void Prefix(Pickupable __instance)
	//	{
	//		if (!__instance.overrideTechUsed)
	//			return;

	//		CreatureEgg egg = __instance.GetComponent<CreatureEgg>();
	//		if (egg && KnownTech.Contains(egg.eggType))
	//		{
	//			__instance.ResetTechTypeOverride();
	//			egg.Subscribe(false); // just in case
	//		}
	//	}
	//}
}