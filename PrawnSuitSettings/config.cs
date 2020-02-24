using Common.Configuration;

namespace PrawnSuitSettings
{
	[AddToConsole("pss")]
	[Options.Name("Prawn Suit Settings")]
	class ModConfig: Config
	{
		public class CollisionSelfDamageSettings
		{
			[Options.Field("Damage from collisions")]
			[Field.CustomAction(typeof(CollisionSelfDamage.SettingChanged))]
			public readonly bool enabled = false;

			public readonly float speedMinimumForDamage = 20f;
			public readonly float mirroredSelfDamageFraction = 0.1f;
		}
		public readonly CollisionSelfDamageSettings collisionSelfDamage = new CollisionSelfDamageSettings();

		public class ArmsEnergyUsageSettings
		{
			[Options.Field("Arms additional energy usage")]
			[Field.CustomAction(typeof(ArmsEnergyUsage.SettingChanged))]
			public readonly bool enabled = false;

			public readonly float drillArm = 0.3f;
			public readonly float grapplingArmShoot = 0.5f;
			public readonly float grapplingArmPull = 0.2f;

			// vanilla energy usage
			public readonly float torpedoArm = 0f;
			public readonly float clawArm = 0.1f;
		}
		public readonly ArmsEnergyUsageSettings armsEnergyUsage = new ArmsEnergyUsageSettings();

		[Options.Field("Propulsion arm 'ready' animation")]
		public readonly bool readyAnimationForPropulsionCannon = true;

		[Options.Field("Toggleable drill arm")]
		public readonly bool toggleableDrillArm = false;

		[Options.Field("Auto pickup resources after drilling")]
		[Field.CustomAction(typeof(Common.HarmonyHelper.UpdateOptionalPatches))]
		public readonly bool autoPickupDrillableResources = true;
	}
}