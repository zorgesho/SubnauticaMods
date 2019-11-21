using Common.Configuration;

namespace DayNightSpeed
{
	class ModConfig: Config
	{
		public const float hungerTimeInitial = 2520f; // secs for 100->0 hunger
		public const float thristTimeInitial = 1800f; // secs for 100->0 thrist

		[AddToConsole]
		[Field.Bounds(0f, 100f)]
		[Field.CustomAction(typeof(DayNightSpeedControl.SettingChanged))]
		[Options.Field("Day/night speed")]
		[Options.Choice("4x slower", 0.25f, "3x slower", 0.33f, "2x slower", 0.5f, "1.5x slower", 0.66f, "Normal speed", 1.0f, "1.5x faster", 1.5f, "2x faster", 2.0f)]
		public readonly float dayNightSpeed = 1.0f;

		public readonly bool clampSpeed = true;
		//public float getDayNightSpeedClamped() => clampSpeed && dayNightSpeed < 1.0f? 1.0f: dayNightSpeed;
		
		public float getDayNightSpeed2() => clampSpeed && dayNightSpeed < 1.0f? dayNightSpeed: 1.0f;
		
		public float getDayNightSpeedInverse() =>
			(clampSpeed && dayNightSpeed > 0 && dayNightSpeed < 1.0f)? 1.0f/dayNightSpeed: dayNightSpeed;

		public readonly float hungerTime = hungerTimeInitial;
		public readonly float thristTime = thristTimeInitial;
		
		public readonly float test = 100f;

		[AddToConsole]
		public readonly float light = 1.0f;
	}
}