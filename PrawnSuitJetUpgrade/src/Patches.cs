using Harmony;
using UnityEngine;

using Common;

namespace PrawnSuitJetUpgrade
{
	[HarmonyPatch(typeof(Exosuit), "FixedUpdate")]
	static class Exosuit_FixedUpdate_Patch
	{
		static void Postfix(Exosuit __instance)
		{
			var ex = __instance;

			if (ex.IsUnderwater() || !ex.jumpJetsUpgraded || !ex.jetsActive || ex.thrustPower == 0f)
				return;

			float d = 0.8f + ex.thrustPower * 0.2f;
			float d2 = Mathf.Clamp01(Mathf.Max(0f, -ex.useRigidbody.velocity.y) / 6f) + 1f;
			ex.useRigidbody.AddForce(Vector3.up * Main.config.jetPowerAboveWater * d * d2, ForceMode.Acceleration);

			// consume more thrust above water
			float consumeMore = Time.fixedDeltaTime * ex.thrustConsumption * Main.config.additionalThrustConsumptionAboveWater;
			ex.thrustPower = Mathf.Clamp01(ex.thrustPower - consumeMore);

			// consume more power above water
			ex.ConsumeEngineEnergy(Main.config.additionalPowerConsumptionAboveWater * Time.fixedDeltaTime);
		}
	}


	[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
	static class Exosuit_OnUpgradeModuleChange_Patch
	{
		const float initialThrustConsumption = 0.09f;

		static void Postfix(Exosuit __instance, TechType techType)
		{
			if (techType != PrawnThrustersOptimizer.TechType)
				return;

			int count = __instance.modules.GetCount(PrawnThrustersOptimizer.TechType);
			float efficiency = 1f + System.Math.Sign(count) * (Main.config.increasedThrustReserve - 1f);

			__instance.thrustConsumption = initialThrustConsumption / efficiency;

			L10n.str(L10n.ids_thrustersEfficiency).format(efficiency).onScreen();
		}
	}
}