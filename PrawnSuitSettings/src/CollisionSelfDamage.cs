using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace PrawnSuitSettings
{
	// Enables damage to prawn suit from collisions
	static class CollisionSelfDamage
	{
		public class SettingChanged: Config.Field.IAction
		{ public void action() => Object.FindObjectsOfType<Exosuit>()?.forEach(refresh); }

		static void refresh(Exosuit exosuit)
		{
			if (!exosuit)
				return;

			var damage = exosuit.GetComponent<DealDamageOnImpact>();

			damage.mirroredSelfDamage = Main.config.collisionSelfDamage.enabled;
			damage.speedMinimumForSelfDamage = Main.config.collisionSelfDamage.speedMinimumForDamage;

#if GAME_SN // BZ doesn't have TechType.VehicleArmorPlating
			float damageReduction = Mathf.Pow(0.5f, exosuit.modules.GetCount(TechType.VehicleArmorPlating));
#elif GAME_BZ
			float damageReduction = 1.0f;
#endif
			damage.mirroredSelfDamageFraction = Main.config.collisionSelfDamage.mirroredSelfDamageFraction * damageReduction;

			$"CollisionSelfDamage: {damage.mirroredSelfDamage} {damage.speedMinimumForSelfDamage} {damage.mirroredSelfDamageFraction}".logDbg();
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => Main.config.collisionSelfDamage.enabled;

			[HarmonyPostfix, HarmonyPatch(typeof(Exosuit), "Start")]
			static void Exosuit_Start_Postfix(Exosuit __instance) => refresh(__instance);
#if GAME_SN
			[HarmonyPostfix, HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
			static void Exosuit_OnUpgradeModuleChange_Postfix(Exosuit __instance, TechType techType)
			{
				if (techType == TechType.VehicleArmorPlating)
					refresh(__instance);
			}
#endif
		}
	}
}