using UnityEngine;
using Harmony;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	// TODO: find better place
	[HarmonyPatch(typeof(Player), "Start")]
	static class Player_Start_Patch
	{
		static void Postfix() => CraftData.energyCost[TechType.ExosuitGrapplingArmModule] = Main.config.armEnergyDrain.grapplingArmShoot;
	}

	
	static class ArmsEnergyDrainPatches
	{
		const float sqrMagnitudeGrapplingArm = 16f;	// if hook attached and its sqr length less than it, then dont drain power

		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				$"ArmsEnergyDrain {Main.config.armEnergyDrain.armsEnergyAdditionalDrain}".onScreen();
			}
		}

		static void exosuitConsumeEnergyForArmOrStop(Exosuit exosuit, IExosuitArm arm, float energyPerSec)
		{
			if (exosuit != null)
			{
				float needEnergy = energyPerSec * Time.deltaTime;
				if (exosuit.HasEnoughEnergy(needEnergy))
					exosuit.ConsumeEnergy(needEnergy);
				else
					arm.OnUseUp(out _);
			}
		}
		
		
		[HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.Update")]
		static class ExosuitDrillArm_Update_Patch
		{
			static void Postfix(ExosuitDrillArm __instance)
			{
				if (__instance.drilling)
					exosuitConsumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, Main.config.armEnergyDrain.drillArm);
			}
		}


		[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
		static class ExosuitGrapplingArm_FixedUpdate_Patch
		{
			static void Postfix(ExosuitGrapplingArm __instance)
			{
				if (__instance.hook.attached && (__instance.hook.transform.position - __instance.front.position).sqrMagnitude > sqrMagnitudeGrapplingArm)
					exosuitConsumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, Main.config.armEnergyDrain.grapplingArmPull);
			}
		}
	}
}