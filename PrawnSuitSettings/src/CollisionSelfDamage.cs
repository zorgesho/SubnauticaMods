using Harmony;
using UnityEngine;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	// Enables damage to prawn suit from collisions
	static class CollisionSelfDamage
	{
		public class SettingChanged: Config.Field.IAction
		{
			public void action() => Object.FindObjectsOfType<Exosuit>()?.forEach(ex => refresh(ex));
		}


		static void refresh(Exosuit exosuit)
		{
			if (exosuit == null)
				return;

			DealDamageOnImpact damage = exosuit.GetComponent<DealDamageOnImpact>();

			damage.mirroredSelfDamage = Main.config.collisionSelfDamage.enabled;
			damage.speedMinimumForSelfDamage = Main.config.collisionSelfDamage.speedMinimumForDamage;

			int armorCount = exosuit.modules.GetCount(TechType.VehicleArmorPlating);
			damage.mirroredSelfDamageFraction =
				Main.config.collisionSelfDamage.mirroredSelfDamageFraction * Mathf.Pow(0.5f, armorCount);

			$"CollisionSelfDamage: {damage.mirroredSelfDamage} {damage.speedMinimumForSelfDamage} {damage.mirroredSelfDamageFraction}".logDbg();
		}


		[HarmonyHelper.OptionalPatch]
		[HarmonyPatch(typeof(Exosuit), "Start")]
		static class Exosuit_Start_Patch
		{
			static bool Prepare() => Main.config.collisionSelfDamage.enabled;

			static void Postfix(Exosuit __instance) => refresh(__instance);
		}


		[HarmonyHelper.OptionalPatch]
		[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
		static class Exosuit_OnUpgradeModuleChange_Patch
		{
			static bool Prepare() => Main.config.collisionSelfDamage.enabled;

			static void Postfix(Exosuit __instance, TechType techType)
			{
				if (techType == TechType.VehicleArmorPlating)
					refresh(__instance);
			}
		}
	}
}