using Common.Configuration;

namespace PrawnSuitGrapplingArmUpgrade
{
	[AddToConsole("ps_ga")]
	class ModConfig: Config
	{
		[Field.Bounds(Min: 0)]
		public readonly float armCooldown = 0.5f; // default: 2.0f

		[Options.Field]
		[Field.Bounds(35f, 100f)]
		public readonly float hookMaxDistance = 50f; // default: 35f
		
		[Options.Field]
		[Field.Bounds(25f, 100f)]
		public readonly float hookSpeed = 50f; // default: 25f

		[Options.Field]
		[Field.Bounds(15f, 100f)]
		public readonly float acceleration = 20f; // default: 15f
		
		[Options.Field]
		[Field.Bounds(400f, 800f)]
		public readonly float force = 600f; // default: 400f
	}
}