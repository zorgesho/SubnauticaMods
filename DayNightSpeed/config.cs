using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	[AddToConsole("dns")] // warning: "dns" is used in daynightspeed command
	[Options.Name("Day/Night Speed")]
	class ModConfig: Config
	{
		[Field.Range(0f, 100f)]
		[Field.CustomAction(typeof(DayNightSpeedControl.SettingChanged))]
		[Options.Field("Day/night speed")]
		[Options.Choice("Normal speed", 1.0f, "1.5x faster", 1.5f, "2x faster", 2.0f, "4x slower", 0.25f, "3x slower", 0.33f, "2x slower", 0.5f, "1.5x slower", 0.66f)]
		public readonly float dayNightSpeed = 1.0f;

		[Field.Range(Min:0.01f)] public readonly float speedHungerThrist   = 1.0f;
		[Field.Range(Min:0.01f)] public readonly float speedPlantsGrow     = 1.0f;
		[Field.Range(Min:0.01f)] public readonly float speedEggsHatching   = 1.0f;
		[Field.Range(Min:0.01f)] public readonly float speedCreaturesGrow  = 1.0f;
		[Field.Range(Min:0.01f)] public readonly float speedMedkitInterval = 1.0f;
		[Field.Range(Min:0.01f)] public readonly float speedPowerCharge    = 1.0f;
		[Field.Range(Min:0.01f)] public readonly float speedPowerConsume   = 1.0f;

		// obsolete inverted multipliers (v1.0.0)
#pragma warning disable CS0414 // unused field
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multHungerThrist   = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multPlantsGrow     = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multEggsHatching   = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multCreaturesGrow  = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multMedkitInterval = 1.0f;
#pragma warning restore

		// updating v1.0.0 -> v1.1.0
		protected override void onLoad()
		{
			var varNames = new string[] { "HungerThrist", "PlantsGrow", "EggsHatching", "CreaturesGrow", "MedkitInterval" };

			try
			{
				// using reflection to avoid copy/paste and keep new params readonly
				foreach (var varName in varNames)
				{
					float val = GetType().field("mult" + varName).GetValue(this).toFloat();

					if (val != 1.0f)
						GetType().field("speed" + varName).SetValue(this, 1.0f / val);
				}
			}
			catch (System.Exception e) { Log.msg(e); }
		}

#if DEBUG
		public class DbgCfg
		{
			public readonly bool showGoals = false;
			public readonly bool showSurvivalStats = false;
			public readonly bool showToggleLightStats = false;
			public readonly bool showWaterParkCreatures = false;
		}
		public readonly DbgCfg dbgCfg = new DbgCfg();
#endif
	}
}