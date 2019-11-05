using System;
using Common.Configuration;

namespace PrawnSuitSettings
{
	[Options.Name("Prawn Suit Settings")]
	[AddToConsole("pss")]
	class ModConfig: Config
	{
		public class CollisionSelfDamageSettings
		{
			[NonSerialized]
			[Options.Field("Damage from collisions")]
			[Field.CustomAction(typeof(CollisionSelfDamage.SettingChanged))]
			public readonly bool _enabled_0 = true; // can't use just 'enabled' now to avoid name collision in mod options
			
			public bool enabled = true;

			public readonly float speedMinimumForDamage = 20f;
			public readonly float mirroredSelfDamageFraction = 0.1f;
		}
		
		public class ArmsEnergyUsageSettings
		{
			[NonSerialized]
			[Options.Field("Arms additional energy usage")]
			[Field.CustomAction(typeof(ArmsEnergyUsage.SettingChanged))]
			public readonly bool _enabled_1 = true; // temporary workaround
			
			public bool enabled = true;
			
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