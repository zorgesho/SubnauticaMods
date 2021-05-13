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
		{ public void action() => refresh(); }

		static void setEnergyCost(TechType techType, float energyCost)
		{
#if GAME_SN
			CraftData.energyCost[techType] = energyCost;
#elif GAME_BZ
			if (TechData.TryGetValue(techType, out JsonValue value))
				value.setDouble(TechData.propertyEnergyCost, energyCost);
#endif
		}

		public static void refresh()
		{																													$"ArmsEnergyUsage: {Main.config.armsEnergyUsage.enabled}".logDbg();
			float grapplingArmEnergyCost = Main.config.armsEnergyUsage.enabled? Main.config.armsEnergyUsage.grapplingArmShoot: 0f;

			setEnergyCost(TechType.ExosuitGrapplingArmModule, grapplingArmEnergyCost);

			if (TechTypeHandler.TryGetModdedTechType("GrapplingArmUpgradeModule", out TechType upgradedGrapplingArm))
				setEnergyCost(upgradedGrapplingArm, grapplingArmEnergyCost);

			setEnergyCost(TechType.ExosuitTorpedoArmModule, Main.config.armsEnergyUsage.enabled? Main.config.armsEnergyUsage.torpedoArm: 0f);
			setEnergyCost(TechType.ExosuitClawArmModule, Main.config.armsEnergyUsage.enabled? Main.config.armsEnergyUsage.clawArm: 0.1f);
		}

		static bool consumeArmEnergy(MonoBehaviour exosuitArm, float energyPerSec)
		{																													$"ArmsEnergyUsage: trying to consume {energyPerSec} energy for {exosuitArm}".logDbg();
			return exosuitArm.GetComponentInParent<Exosuit>().ConsumeEnergy(energyPerSec * Time.deltaTime);
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => Main.config.armsEnergyUsage.enabled;

			[HarmonyPostfix, HarmonyPatch(typeof(ExosuitDrillArm), "IExosuitArm.Update")] // energy usage for drill arm
			static void ExosuitDrillArm_Update_Postfix(ExosuitDrillArm __instance)
			{
				if (__instance.drilling && !consumeArmEnergy(__instance, Main.config.armsEnergyUsage.drillArm))
				{
					__instance.gameObject.GetComponent<ToggleableDrillArmPatch.ArmToggle>()?.setUsingArm(false);
					(__instance as IExosuitArm).OnUseUp(out _);
				}
			}

			[HarmonyPostfix, HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")] // energy usage for grappling arm
			static void ExosuitGrapplingArm_FixedUpdate_Postfix(ExosuitGrapplingArm __instance)
			{
				const float sqrMagnitudeGrapplingArm = 16f; // if hook attached and its sqr length less than that, then don't consume power

				if (__instance.hook.attached && (__instance.hook.transform.position - __instance.front.position).sqrMagnitude > sqrMagnitudeGrapplingArm)
					if (!consumeArmEnergy(__instance, Main.config.armsEnergyUsage.grapplingArmPull))
						(__instance as IExosuitArm).OnUseUp(out _);
			}
#if GAME_BZ
			[HarmonyPostfix, HarmonyPatch(typeof(TechData), "Initialize")]
			static void TechData_Initialize_Postfix() => refresh();
#endif
		}
#if GAME_BZ
		static void setDouble(this JsonValue jv, int id, double val)
		{
			if (!jv.CheckType(JsonValue.Type.Object))
				return;

			if (!jv.dataObject.TryGetValue(id, out JsonValue doubleValue))
			{
				doubleValue = new JsonValue(JsonValue.Type.Double);
				jv.Add(id, doubleValue);
			}

			doubleValue.SetDouble(val);
		}
#endif
	}
}