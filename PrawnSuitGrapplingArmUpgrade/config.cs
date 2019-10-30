using Common.Configuration;

namespace PrawnSuitGrapplingArmUpgrade
{
	[AddToConsole("ps_ga")]
	class ModConfig: Config
	{
		[Field.Bounds(Min: 0)]
		public float armCooldown = 0.2f;

		[Options.Field]
		[Field.Bounds(1, 900)]
		public float armLength = 42f;
		
		[Options.Field]
		[Field.Bounds(1, 100)]
		public float field15 = 15f;
		
		[Options.Field]
		[Field.Bounds(1, 100)]
		public float field25 = 25f;
		
		[Options.Field]
		[Field.Bounds(1, 900)]
		public float field400 = 400f;
	}
}