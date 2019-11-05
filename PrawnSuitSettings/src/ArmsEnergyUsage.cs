using UnityEngine;
using Harmony;

using SMLHelper.V2.Handlers;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	static class ArmsEnergyUsage
	{
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				Main.config.armsEnergyUsage.enabled = Main.config.armsEnergyUsage._enabled_1;
				refresh();
			}
		}


		public static void refresh()
		{																													$"ArmsEnergyUsage: {Main.config.armsEnergyUsage.enabled}".logDbg();
			float grapplingArmEnergyCost = Main.config.armsEnergyUsage.enabled? Main.config.armsEnergyUsage.grapplingArmShoot: 0f;

			CraftData.energyCost[TechType.ExosuitGrapplingArmModule] = grapplingArmEnergyCost;
			
			if (TechTypeHandler.TryGetModdedTechType("GrapplingArmUpgradeModule", out TechType upgradedGrapplingArm))
				CraftData.energyCost[upgradedGrapplingArm] = grapplingArmEnergyCost;

			CraftData.energyCost[TechType.ExosuitTorpedoArmModule] = Main.config.armsEnergyUsage.torpedoArm;
			CraftData.energyCost[TechType.ExosuitClawArmModule] = Main.config.armsEnergyUsage.clawArm;
		}


		static void consumeEnergyForArmOrStop(Exosuit exosuit, IExosuitArm arm, float energyPerSec)
		{
			if (exosuit != null)
			{																												$"ArmsEnergyUsage: trying to consume {energyPerSec} energy".logDbg();
				if (!exosuit.ConsumeEnergy(energyPerSec * Time.deltaTime))
					arm.OnUseUp(out _);
			}
		}

		// Energy usage from drill arm
		[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.Update")]
		static class ExosuitDrillArm_Update_Patch
		{
			static void Postfix(ExosuitDrillArm __instance)
			{
				if (Main.config.armsEnergyUsage.enabled && __instance.drilling)
					consumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, Main.config.armsEnergyUsage.drillArm);
			}
		}

		// Energy usage from grappling arm
		[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
		static class ExosuitGrapplingArm_FixedUpdate_Patch
		{
			const float sqrMagnitudeGrapplingArm = 16f; // if hook attached and its sqr length less than that, then don't consume power
			
			static void Postfix(ExosuitGrapplingArm __instance)
			{
				if (Main.config.armsEnergyUsage.enabled &&
					__instance.hook.attached &&
					(__instance.hook.transform.position - __instance.front.position).sqrMagnitude > sqrMagnitudeGrapplingArm)
				{
					consumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, Main.config.armsEnergyUsage.grapplingArmPull);
				}
			}
		}
	}
}