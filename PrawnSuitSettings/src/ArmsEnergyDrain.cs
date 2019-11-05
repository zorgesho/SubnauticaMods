using UnityEngine;
using Harmony;

using SMLHelper.V2.Handlers;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	static class ArmsEnergyDrain
	{
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				refresh();

				$"ArmsEnergyDrain {Main.config.armEnergyDrain.drainEnabled}".onScreen();
			}
		}


		public static void refresh()
		{
			float grapplingArmEnergyCost = Main.config.armEnergyDrain.drainEnabled? Main.config.armEnergyDrain.grapplingArmShoot: 0f;

			CraftData.energyCost[TechType.ExosuitGrapplingArmModule] = grapplingArmEnergyCost;
			
			if (TechTypeHandler.TryGetModdedTechType("GrapplingArmUpgradeModule", out TechType upgradedGrapplingArm))
				CraftData.energyCost[upgradedGrapplingArm] = grapplingArmEnergyCost;

			CraftData.energyCost[TechType.ExosuitTorpedoArmModule] = Main.config.armEnergyDrain.torpedoArm;
			CraftData.energyCost[TechType.ExosuitClawArmModule] = Main.config.armEnergyDrain.clawArm;
		}


		static void consumeEnergyForArmOrStop(Exosuit exosuit, IExosuitArm arm, float energyPerSec)
		{
			if (exosuit != null)
			{
				if (!exosuit.ConsumeEnergy(energyPerSec * Time.deltaTime))
					arm.OnUseUp(out _);
			}
		}


		[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.Update")]
		static class ExosuitDrillArm_Update_Patch
		{
			static void Postfix(ExosuitDrillArm __instance)
			{
				if (Main.config.armEnergyDrain.drainEnabled && __instance.drilling)
					consumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, Main.config.armEnergyDrain.drillArm);
			}
		}


		[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
		static class ExosuitGrapplingArm_FixedUpdate_Patch
		{
			const float sqrMagnitudeGrapplingArm = 16f; // if hook attached and its sqr length less than that, then don't consume power
			
			static void Postfix(ExosuitGrapplingArm __instance)
			{
				if (Main.config.armEnergyDrain.drainEnabled &&
					__instance.hook.attached &&
					(__instance.hook.transform.position - __instance.front.position).sqrMagnitude > sqrMagnitudeGrapplingArm)
				{
					consumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, Main.config.armEnergyDrain.grapplingArmPull);
				}
			}
		}
	}
}