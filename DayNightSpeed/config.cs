using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	class ModConfig: Config
	{
		[AddToConsole]
		[Field.Bounds(0f, 100f)]
		[Field.CustomAction(typeof(DayNightSpeedControl.SettingChanged))]
		[Options.Field("Day/night speed")]
		[Options.Choice("4x slower", 0.25f, "3x slower", 0.33f, "2x slower", 0.5f, "1.5x slower", 0.66f, "Normal speed", 1.0f, "1.5x faster", 1.5f, "2x faster", 2.0f)]
		public readonly float dayNightSpeed = 1.0f;

		public const float hungerTimeInitial = 2520f; // default secs for 100->0 hunger
		public const float thristTimeInitial = 1800f; // default secs for 100->0 thrist

		float hungerTime = hungerTimeInitial;
		float thristTime = thristTimeInitial;
		
		// used in transpilers, so we need methods instead of properties
		public float getHungerTime() => hungerTime;
		public float getThristTime() => thristTime;

		[AddToConsole] public readonly float multPlantsGrow     = 1.0f;
		[AddToConsole] public readonly float multHungerThrist   = 1.0f;
		[AddToConsole] public readonly float multEggsHatching   = 1.0f;
		[AddToConsole] public readonly float multMedkitInterval = 1.0f;

		public void updateValues(float dnSpeed)
		{
			if (dnSpeed > float.Epsilon)
			{
				hungerTime = hungerTimeInitial * multHungerThrist / dnSpeed;
				thristTime = thristTimeInitial * multHungerThrist / dnSpeed;							$"Hunger/thrist times changed: {hungerTime} {thristTime}".logDbg();
			}
		}

		// for transpilers
		public float getDayNightSpeedClamped01() => (dayNightSpeed > float.Epsilon && dayNightSpeed < 1.0f)? dayNightSpeed: 1.0f;

#if DEBUG
		[AddToConsole]
		public class DbgCfg
		{
			public readonly bool showGoals = true;
			public readonly bool showSurvivalStats = false;
			public readonly bool showToggleLightStats = false;
		}
		public readonly DbgCfg dbgCfg = new DbgCfg();
#endif
	}
}