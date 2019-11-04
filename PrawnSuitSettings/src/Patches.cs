using System;

using Harmony;

using UnityEngine;

namespace PrawnSuitSettings
{
	// misc staff not directly connected with player. TODO: find better place
	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("Start")]
	class Player_Start_Patch
	{
		private static void Postfix(Player __instance)
		{
			CraftData.energyCost[TechType.ExosuitGrapplingArmModule] = PrawnSuitArmsEnergyDrain.energyDrainGrapplingArmShoot;
		}
	}

	// get access to prawn suit stuff when docked in moonpool
	[HarmonyPatch(typeof(Exosuit))]
	[HarmonyPatch("UpdateColliders")]
	class Exosuit_UpdateColliders_Patch
	{
		private static void Postfix(Exosuit __instance)
		{
			string[] enabledColliders = {"Storage", "UpgradeConsole", "BatteryLeft", "BatteryRight"};

			for (int i = 0; i < __instance.disableDockedColliders.Length; i++)
			{
				if (Array.IndexOf(enabledColliders, __instance.disableDockedColliders[i].name) != -1)
					__instance.disableDockedColliders[i].enabled = true;
			}
		}
	}

	// Enables damage to prawn suit from collisions with terrain
	class PrawnSuitSelfDamage
	{
		static float speedMinimumForDamage = 20f;
		static float mirroredSelfDamageFraction = 0.1f;
		
		[HarmonyPatch(typeof(Exosuit))]
		[HarmonyPatch("Start")]
		class Exosuit_Start_Patch
		{
			private static void Postfix(Exosuit __instance)
			{
				DealDamageOnImpact damage = __instance.GetComponent<DealDamageOnImpact>();

				damage.mirroredSelfDamage = true;
				damage.speedMinimumForDamage = speedMinimumForDamage;
				damage.mirroredSelfDamageFraction = mirroredSelfDamageFraction;
			}
		}

		[HarmonyPatch(typeof(Exosuit))]
		[HarmonyPatch("OnUpgradeModuleChange")]
		class Exosuit_OnUpgradeModuleChange_Patch
		{
			private static void Postfix(Exosuit __instance, int slotID, TechType techType, bool added)
			{
				if (techType == TechType.VehicleArmorPlating)
				{
					DealDamageOnImpact component = __instance.GetComponent<DealDamageOnImpact>();
					component.mirroredSelfDamageFraction = mirroredSelfDamageFraction * Mathf.Pow(0.5f, (float)__instance.modules.GetCount(TechType.VehicleArmorPlating));
				}
			}
		}
	}

	class PrawnSuitArmsEnergyDrain
	{
		static float energyDrainDrillArm = 0.3f;
		
		static float sqrMagnitudeGrapplingArm = 16f;	// if hook attached and its sqr length less than it, then dont drain power
		static float energyDrainGrapplingArmPull = 0.2f;
		public static float energyDrainGrapplingArmShoot = 0.5f; // used somewhere else for init

		static void exosuitConsumeEnergyForArmOrStop(Exosuit exosuit, IExosuitArm arm, float energyPerSec)
		{
			if (exosuit != null)
			{
				float needEnergy = energyPerSec * Time.deltaTime;
				if (exosuit.HasEnoughEnergy(needEnergy))
					exosuit.ConsumeEnergy(needEnergy);
				else
					arm.OnUseUp(out needEnergy);
			}
		}
		
		
		[HarmonyPatch(typeof(ExosuitDrillArm))]
		[HarmonyPatch("IExosuitArm.Update")]
		class ExosuitDrillArm_Update_Patch
		{
			private static void Postfix(ExosuitDrillArm __instance, ref Quaternion aimDirection)
			{
				if (__instance.drilling)
					exosuitConsumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, energyDrainDrillArm);
			}
		}


		[HarmonyPatch(typeof(ExosuitGrapplingArm))]
		[HarmonyPatch("FixedUpdate")]
		class ExosuitGrapplingArm_FixedUpdate_Patch
		{
			private static void Postfix(ExosuitGrapplingArm __instance)
			{
				if (__instance.hook.attached && (__instance.hook.transform.position - __instance.front.position).sqrMagnitude > sqrMagnitudeGrapplingArm)
					exosuitConsumeEnergyForArmOrStop(__instance.GetComponentInParent<Exosuit>(), __instance, energyDrainGrapplingArmPull);
			}
		}
	}

}