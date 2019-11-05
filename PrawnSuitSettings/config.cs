using Common.Configuration;

namespace PrawnSuitSettings
{
	[Options.Name("Prawn Suit Settings")]
	[AddToConsole("pss")]
	class ModConfig: Config
	{
		[Options.Field]
		public readonly bool accessToPrawnSuitPartsWhenDocked = true;
		
		[Options.Field]
		public readonly bool passivePropulsionCannon = true;
		
		public class CollisionSelfDamageSettings
		{
			[Options.Field("CollisionSelfDamage")]
			[Field.CustomAction(typeof(CollisionSelfDamage.SettingChanged))]
			public readonly bool damageEnabled = true; // can't use just 'enabled' now to avoid name collision in mod options
			
			public readonly float speedMinimumForDamage = 20f;
			public readonly float mirroredSelfDamageFraction = 0.1f;
		}
		
		public class ArmEnergyDrainSettings
		{
			[Options.Field("ArmsEnergyDrain")]
			[Field.CustomAction(typeof(ArmsEnergyDrain.SettingChanged))]
			public readonly bool drainEnabled = true;
			
			public readonly float drillArm = 0.3f;
			public readonly float grapplingArmShoot = 0.5f;
			public readonly float grapplingArmPull = 0.2f;
			
			// vanilla energy usage
			public readonly float torpedoArm = 0f;
			public readonly float clawArm = 0.1f;
		}
		
		public readonly CollisionSelfDamageSettings collisionSelfDamage = new CollisionSelfDamageSettings();
		public readonly ArmEnergyDrainSettings armEnergyDrain = new ArmEnergyDrainSettings();
	}
}