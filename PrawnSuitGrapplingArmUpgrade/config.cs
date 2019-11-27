using Common.Configuration;

namespace PrawnSuitGrapplingArmUpgrade
{
	[AddToConsole("ps_ga")]
	class ModConfig: Config
	{
		[Field.Bounds(0f, 5f)]	   public readonly float armCooldown = 0.5f;	// default: 2.0f
		[Field.Bounds(35f, 100f)]  public readonly float hookMaxDistance = 50f; // default: 35f
		[Field.Bounds(25f, 100f)]  public readonly float hookSpeed = 50f;		// default: 25f
		[Field.Bounds(15f, 50f)]   public readonly float acceleration = 20f;	// default: 15f
		[Field.Bounds(400f, 800f)] public readonly float force = 600f;			// default: 400f
	}
}