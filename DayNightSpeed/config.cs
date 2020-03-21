using System;
using System.Linq;

using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	[AddToConsole("dns")] // warning: "dns" is used in daynightspeed command
	[Options.Name("Day/Night Speed")]
	class ModConfig: Config
	{
		const float dayNightSecs = 1200f;

		[Slider_0_100]
		[Field.Range(0f, 100f)] // for UI minimum is 0.01f
		[Field.CustomAction(typeof(DayNightSpeedControl.SettingChanged))]
		[Options.Field("Day/night speed")]
		public readonly float dayNightSpeed = 1.0f;

		[Options.Field("Use additional multipliers")]
		[Field.CustomAction(typeof(SpeedsHider))]
		public readonly bool useAuxSpeeds = false;

		[Options.Field("Hunger/thrist")]
		[Slider_0_100][Range_001_100][HideableSpeed]
		public readonly float speedHungerThrist = 1.0f;
		public float auxSpeedHungerThrist => useAuxSpeeds? speedHungerThrist: 1.0f;

		[Options.Field("Plants growth")]
		[UpdateOptionalPatches]
		[Slider_0_100][Range_001_100][HideableSpeed]
		public readonly float speedPlantsGrow = 1.0f;

		[Options.Field("Eggs hatching")]
		[UpdateOptionalPatches]
		[Slider_0_100][Range_001_100][HideableSpeed]
		public readonly float speedEggsHatching = 1.0f;

		[Options.Field("Creatures growth")]
		[UpdateOptionalPatches]
		[Slider_0_100][Range_001_100][HideableSpeed]
		public readonly float speedCreaturesGrow = 1.0f;

		[Options.Field("Medkit fabrication")]
		[UpdateOptionalPatches]
		[Slider_0_100][Range_001_100][HideableSpeed]
		public readonly float speedMedkitInterval = 1.0f;

		[Options.Field("Charging/generating power")]
		[Slider_0_100][Range_001_100][HideableSpeed]
		public readonly float speedPowerCharge = 1.0f;
		public float auxSpeedPowerCharge => useAuxSpeeds? speedPowerCharge: 1.0f;

		[Range_001_100]
		public readonly float speedPowerConsume = 1.0f; // no need to add it to the options
		public float auxSpeedPowerConsume => useAuxSpeeds? speedPowerConsume: 1.0f;

		#region aux speeds hider
		class SpeedsHider: Field.ICustomAction, Options.Components.Hider.IVisibilityChecker
		{
			public bool isVisible() => Main.config.useAuxSpeeds;

			public void customAction()
			{
				Options.Components.Hider.setVisible("speeds", Main.config.useAuxSpeeds);
				HarmonyHelper.updateOptionalPatches();
			}
		}

		class HideableSpeed: Options.HideableAttribute
			{ public HideableSpeed(): base(typeof(SpeedsHider), "speeds") {} }

		class UpdateOptionalPatches: Field.CustomActionAttribute
			{ public UpdateOptionalPatches(): base(typeof(HarmonyHelper.UpdateOptionalPatches)) {} }
		#endregion

		#region nonlinear slider
		class SliderValue_0_100: Options.Components.NonlinearSliderValue
		{
			SliderValue_0_100()
			{
				addValueInfo(0.5f, 1.0f, "{0:F2}", "{0:F1}");
				addValueInfo(0.7f, 3.0f);
				addValueInfo(0.8f, 10.0f, "{0:F1}", "{0:F0}");
			}

			public override float ConvertToDisplayValue(float value) =>
				(float)Math.Max(0.01f, Math.Round(base.ConvertToDisplayValue(value), 4));
		}

		class Range_001_100: Field.RangeAttribute
			{ public Range_001_100(): base(0.01f, 100f) {} }

		class Slider_0_100: Options.SliderAttribute
			{ public Slider_0_100(): base(DefaultValue: 1.0f, CustomValueType: typeof(SliderValue_0_100)) {} }
		#endregion

		#region version updates
		protected override void onLoad()
		{
			_updateTo110();
			_updateTo120();
		}

		#region v1.0.0 -> v1.1.0
#pragma warning disable CS0414 // unused field
		// obsolete inverted multipliers (v1.0.0)
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multHungerThrist   = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multPlantsGrow     = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multEggsHatching   = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multCreaturesGrow  = 1.0f;
		[Field.LoadOnly] [Field.Range(Min:0.01f)] readonly float multMedkitInterval = 1.0f;
#pragma warning restore

		// variables are renamed (mult* -> speed*) and inverted (new = 1.0f/old)
		void _updateTo110()
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
			catch (Exception e) { Log.msg(e); }
		}
		#endregion

		#region v1.0.0/1.1.0 -> v1.2.0
		int __cfgVer = 0;

		// if we loading this config for the first time and some speed variable is not default then we enabling useAuxSpeeds
		void _updateTo120()
		{
			if (__cfgVer == 120)
				return;

			__cfgVer = 120;

			try
			{
				if (GetType().fields().Where(field => field.Name.StartsWith("speed") && !field.GetValue(this).Equals(1.0f)).Count() > 0)
					GetType().field(nameof(useAuxSpeeds)).SetValue(this, true);
			}
			catch (Exception e) { Log.msg(e); }
		}
		#endregion
		#endregion

		#region debug config
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
		#endregion
	}
}