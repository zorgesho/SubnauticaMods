using System;
using Common.Configuration;

namespace DayNightSpeed
{
	class ModConfig: Config
	{
		[NonSerialized]	public const float hunger = 2520f;
		[NonSerialized]	public const float thrist = 1800f;

		[Options.Field]
		[Options.Choice("0.1", "0.2", "0.5")]
		public readonly int speedChoice = 0;

		[AddToConsole]
		[Field.Bounds(0.0001f, 100f)]
		public readonly float dayNightSpeed = 0.5f;

		public readonly bool clampSpeed = true;
		//public float getDayNightSpeedClamped() => clampSpeed && dayNightSpeed < 1.0f? 1.0f: dayNightSpeed;
		
		public float getDayNightSpeed2() => clampSpeed && dayNightSpeed < 1.0f? dayNightSpeed: 1.0f;
		
		public float getDayNightSpeedInverse() =>
			(clampSpeed && dayNightSpeed > 0 && dayNightSpeed < 1.0f)? 1.0f/dayNightSpeed: dayNightSpeed;

		public readonly float hungerSec = hunger;
		public readonly float thristSec = thrist;
		
		public readonly float test = 100f;

		[AddToConsole]
		public readonly float light = 1.0f;
	}
}