using UnityEngine;

using Harmony;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	// Enables damage to prawn suit from collisions
	static class CollisionSelfDamage
	{
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction() => Object.FindObjectsOfType<Exosuit>()?.forEach(ex => refresh(ex));
		}


		static void refresh(Exosuit exosuit)
		{
			if (exosuit != null)
			{
				DealDamageOnImpact damage = exosuit.GetComponent<DealDamageOnImpact>();

				damage.mirroredSelfDamage = Main.config.collisionSelfDamage.damageEnabled;
				damage.speedMinimumForSelfDamage = Main.config.collisionSelfDamage.speedMinimumForDamage;

				int armorCount = exosuit.modules.GetCount(TechType.VehicleArmorPlating);
				damage.mirroredSelfDamageFraction =
					Main.config.collisionSelfDamage.mirroredSelfDamageFraction * Mathf.Pow(0.5f, armorCount);

				$"CollisionSelfDamage: {damage.mirroredSelfDamage} {damage.speedMinimumForSelfDamage} {damage.mirroredSelfDamageFraction}".logDbg();
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