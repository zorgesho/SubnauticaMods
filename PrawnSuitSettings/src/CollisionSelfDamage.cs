using UnityEngine;

using Harmony;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	// Enables damage to prawn suit from collisions with terrain
	static class CollisionSelfDamage
	{
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				foreach (var ex in Object.FindObjectsOfType<Exosuit>())
					refresh(ex);

				$"CollisionSelfDamage {Main.config.collisionSelfDamage.damageEnabled}".onScreen().logDbg();
			}
		}

		static void refresh(Exosuit exosuit)
		{
			if (exosuit != null)
			{
				DealDamageOnImpact damage = exosuit.GetComponent<DealDamageOnImpact>();

				damage.mirroredSelfDamage = Main.config.collisionSelfDamage.damageEnabled;
				damage.speedMinimumForDamage = Main.config.collisionSelfDamage.speedMinimumForDamage;

				int armorCount = exosuit.modules.GetCount(TechType.VehicleArmorPlating);
				damage.mirroredSelfDamageFraction =
					Main.config.collisionSelfDamage.mirroredSelfDamageFraction * Mathf.Pow(0.5f, armorCount);
			}
		}

		[HarmonyPatch(typeof(Exosuit), "Start")]
		static class Exosuit_Start_Patch
		{
			static void Postfix(Exosuit __instance) => refresh(__instance);
		}

		[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
		static class Exosuit_OnUpgradeModuleChange_Patch
		{
			static void Postfix(Exosuit __instance, TechType techType)
			{
				if (techType == TechType.VehicleArmorPlating)
					refresh(__instance);
			}
		}
	}
}