using UnityEngine;
using Harmony;

using Common;

namespace PrawnSuitJetUpgrade
{
	[HarmonyPatch(typeof(Exosuit), "FixedUpdate")]
	class Exosuit_FixedUpdate_Patch
	{
		static void Postfix(Exosuit __instance)
		{
			if (!__instance.IsUnderwater() && __instance.jetsActive && __instance.thrustPower > 0f && __instance.jumpJetsUpgraded)
			{
				float jetPower = Main.config.jetPowerAboveWater;

				float d = 0.8f + __instance.thrustPower * 0.2f;
				float d2 = Mathf.Clamp01(Mathf.Max(0f, -__instance.useRigidbody.velocity.y) / 6f) + 1f;
				__instance.useRigidbody.AddForce(Vector3.up * jetPower * d * d2, ForceMode.Acceleration);

				// consume more thrust above water
				float consumeMore = Time.fixedDeltaTime * __instance.thrustConsumption * Main.config.additionalThrustConsumptionAboveWater;
				__instance.thrustPower = Mathf.Clamp01(__instance.thrustPower - consumeMore);

				// consume more power above water
				__instance.ConsumeEngineEnergy(Main.config.additionalPowerConsumptionAboveWater * Time.fixedDeltaTime);
			}
		}
	}

	
	[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
	class Exosuit_OnUpgradeModuleChange_Patch
	{
		const float initialThrustConsumption = 0.09f;
		
		static void Postfix(Exosuit __instance, TechType techType)
		{
			if (techType == PrawnThrustersOptimizer.TechType)
			{
				int count = __instance.modules.GetCount(PrawnThrustersOptimizer.TechType);
				float resultFactor = 1f / (1f + ((count>0)?1f:0f) * (Main.config.increasedThrustReserve - 1.0f));

				__instance.thrustConsumption = initialThrustConsumption * resultFactor;

				$"Thrusters efficiency is now {(100 + ((count>0)?1f:0f) * (Main.config.increasedThrustReserve - 1) * 100f)}%".onScreen();
			}
		}
	}
}