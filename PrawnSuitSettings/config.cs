using Common.Configuration;

namespace PrawnSuitSettings
{
	[Options.Name("Prawn Suit Settings")]
	[AddToConsole("pss")]
	class ModConfig: Config
	{
		[Options.Field]
		public readonly bool accessToPrawnSuitPartsWhenDocked = true;
		
		public class SelfDamage
		{
			[Options.Field]
			[Field.CustomAction(typeof(CollisionSelfDamagePatches.SettingChanged))]
			public readonly bool prawnSuitCollisionsDamage = true;
			
			public readonly float speedMinimumForDamage = 20f;
			public readonly float mirroredSelfDamageFraction = 0.1f;
		}
		
		public class ArmEnergyDrain
		{
			[Options.Field]
			[Field.CustomAction(typeof(ArmsEnergyDrainPatches.SettingChanged))]
			public readonly bool armsEnergyAdditionalDrain = true;
			
			public readonly float drillArm = 0.3f;
			public readonly float grapplingArmShoot = 0.5f;
			public readonly float grapplingArmPull = 0.2f;
		}
		
		public readonly SelfDamage selfDamage = new SelfDamage();
		public readonly ArmEnergyDrain armEnergyDrain = new ArmEnergyDrain();
	}
}