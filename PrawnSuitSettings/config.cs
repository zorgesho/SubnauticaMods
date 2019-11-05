using Common.Configuration;

namespace PrawnSuitSettings
{
	[Options.Name("Prawn Suit Settings")]
	[AddToConsole("pss")]
	class ModConfig: Config
	{
		public class CollisionSelfDamageSettings
		{
			[Options.Field("Damage from collisions")]
			[Field.CustomAction(typeof(CollisionSelfDamage.SettingChanged))]
			public readonly bool damageEnabled = true; // can't use just 'enabled' now to avoid name collision in mod options
			
			public readonly float speedMinimumForDamage = 20f;
			public readonly float mirroredSelfDamageFraction = 0.1f;
		}
		
		public class ArmsEnergyUsageSettings
		{
			[Options.Field("Arms additional energy usage")]
			[Field.CustomAction(typeof(ArmsEnergyUsage.SettingChanged))]
			public readonly bool usageEnabled = true;
			
			public readonly float drillArm = 0.3f;
			public readonly float grapplingArmShoot = 0.5f;
			public readonly float grapplingArmPull = 0.2f;
			
			// vanilla energy usage
			public readonly float torpedoArm = 0f;
			public readonly float clawArm = 0.1f;
		}
		
		public readonly CollisionSelfDamageSettings collisionSelfDamage = new CollisionSelfDamageSettings();
		public readonly ArmsEnergyUsageSettings armsEnergyUsage = new ArmsEnergyUsageSettings();

		[Options.Field("Full access in moonpool")]
		public readonly bool fullAccessToPrawnSuitWhileDocked = true;
		
		[Options.Field("Propulsion arm 'ready' animation")]
		public readonly bool readyAnimationForPropulsionCannon = false;
	}
}