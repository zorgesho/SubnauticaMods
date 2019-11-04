using Harmony;

using Common;
using Common.Configuration;

namespace PrawnSuitSettings
{
	static class CollisionSelfDamagePatches
	{
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				$"CollisionSelfDamage {Main.config.selfDamage.prawnSuitCollisionsDamage}".onScreen();
			}
		}

		// Enables damage to prawn suit from collisions with terrain
		[HarmonyPatch(typeof(Exosuit), "Start")]
		static class Exosuit_Start_Patch
		{
			static void Postfix(Exosuit __instance)
			{
				DealDamageOnImpact damage = __instance.GetComponent<DealDamageOnImpact>();

				damage.mirroredSelfDamage = true;
				damage.speedMinimumForDamage = Main.config.selfDamage.speedMinimumForDamage;
				damage.mirroredSelfDamageFraction = Main.config.selfDamage.mirroredSelfDamageFraction;
			}
		}

		[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
		static class Exosuit_OnUpgradeModuleChange_Patch
		{
			static void Postfix(Exosuit __instance, TechType techType)
			{
				if (techType == TechType.VehicleArmorPlating)
				{
					DealDamageOnImpact component = __instance.GetComponent<DealDamageOnImpact>();
					component.mirroredSelfDamageFraction = Main.config.selfDamage.mirroredSelfDamageFraction * UnityEngine.Mathf.Pow(0.5f, (float)__instance.modules.GetCount(TechType.VehicleArmorPlating));
				}
			}
		}
	}
}