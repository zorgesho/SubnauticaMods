using Common.Configuration;

namespace DayNightSpeed
{
	[Options.Name("Day/Night Speed")]
	class ModConfig: Config
	{
		[AddToConsole("dns")] // warning: "dns" is used in daynightspeed command
		[Field.Bounds(0f, 100f)]
		[Field.CustomAction(typeof(DayNightSpeedControl.SettingChanged))]
		[Options.Field("Day/night speed")]
		[Options.Choice("Normal speed", 1.0f, "1.5x faster", 1.5f, "2x faster", 2.0f, "4x slower", 0.25f, "3x slower", 0.33f, "2x slower", 0.5f, "1.5x slower", 0.66f)]
		public readonly float dayNightSpeed = 1.0f;

		[AddToConsole("dns")] [Field.Bounds(Min:0.01f)] public readonly float multHungerThrist   = 1.0f;
		[AddToConsole("dns")] [Field.Bounds(Min:0.01f)] public readonly float multPlantsGrow     = 1.0f;
		[AddToConsole("dns")] [Field.Bounds(Min:0.01f)] public readonly float multEggsHatching   = 1.0f;
		[AddToConsole("dns")] [Field.Bounds(Min:0.01f)] public readonly float multCreaturesGrow  = 1.0f;
		[AddToConsole("dns")] [Field.Bounds(Min:0.01f)] public readonly float multMedkitInterval = 1.0f;

#if DEBUG
		[AddToConsole]
		public class DbgCfg
		{
			public readonly bool showGoals = false;
			public readonly bool showSurvivalStats = false;
			public readonly bool showToggleLightStats = false;
		}
		public readonly DbgCfg dbgCfg = new DbgCfg();
#endif
	}
}