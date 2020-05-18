using Harmony;
using UnityEngine;
using SMLHelper.V2.Handlers;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace PrawnSuitSettings
{
	static class ArmsEnergyUsage
	{
		public class SettingChanged: Config.Field.IAction
		{
			public void action() => refresh();
		}


		public static void refresh()
		{																													$"ArmsEnergyUsage: {Main.config.armsEnergyUsage.enabled}".logDbg();
			float grapplingArmEnergyCost = Main.config.armsEnergyUsage.enabled? Main.config.armsEnergyUsage.grapplingArmShoot: 0f;

			CraftData.energyCost[TechType.ExosuitGrapplingArmModule] = grapplingArmEnergyCost;

			if (TechTypeHandler.TryGetModdedTechType("GrapplingArmUpgradeModule", out TechType upgradedGrapplingArm))
				CraftData.energyCost[upgradedGrapplingArm] = grapplingArmEnergyCost;

			CraftData.energyCost[TechType.ExosuitTorpedoArmModule] = Main.config.armsEnergyUsage.enabled? Main.config.armsEnergyUsage.torpedoArm: 0f;
			CraftData.energyCost[TechType.ExosuitClawArmModule] = Main.config.armsEnergyUsage.enabled? Main.config.armsEnergyUsage.clawArm: 0.1f;
		}


		static bool consumeArmEnergy(MonoBehaviour exosuitArm, float energyPerSec)
		{																				$"ArmsEnergyUsage: trying to consume {energyPerSec} energy for {exosuitArm}".logDbg();
			return exosuitArm.GetComponentInParent<Exosuit>().ConsumeEnergy(energyPerSec * Time.deltaTime);
		}

		// Energy usage for drill arm
		[OptionalPatch]
		[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.Update")]
		static class ExosuitDrillArm_Update_Patch
		{
			static bool Prepare() => Main.config.armsEnergyUsage.enabled;

			static void Postfix(ExosuitDrillArm __instance)
			{
				if (__instance.drilling && !consumeArmEnergy(__instance, Main.config.armsEnergyUsage.drillArm))
				{
					__instance.gameObject.GetComponent<PrawnSuitDrillArmToggle>()?.setUsingArm(false);
					(__instance as IExosuitArm).OnUseUp(out _);
				}
			}
		}

		// Energy usage for grappling arm
		[OptionalPatch]
		[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
		static class ExosuitGrapplingArm_FixedUpdate_Patch
		{
			const float sqrMagnitudeGrapplingArm = 16f; // if hook attached and its sqr length less than that, then don't consume power

			static bool Prepare() => Main.config.armsEnergyUsage.enabled;

			static void Postfix(ExosuitGrapplingArm __instance)
			{
				if (__instance.hook.attached && (__instance.hook.transform.position - __instance.front.position).sqrMagnitude > sqrMagnitudeGrapplingArm)
					if (!consumeArmEnergy(__instance, Main.config.armsEnergyUsage.grapplingArmPull))
						(__instance as IExosuitArm).OnUseUp(out _);
			}
		}
	}
}