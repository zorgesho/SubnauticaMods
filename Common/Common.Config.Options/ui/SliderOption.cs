using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public class SliderOption: ModOption
		{
			readonly float  min, max;
			readonly float? defaultValue;
			readonly string valueFormat;

			public SliderOption(Config.Field cfgField, string label, float _min, float _max, float? _defaultValue = null, string _valueFormat = null): base(cfgField, label)
			{
				min = _min;
				max = _max;

				defaultValue = _defaultValue;
				valueFormat  = _valueFormat;
			}

			public override void addOption(Options options)
			{
				options.AddSliderOption(id, label, min, max, cfgField.value.toFloat(), defaultValue, valueFormat);
			}

			public override void onValueChange(EventArgs e)
			{
				cfgField.value = (e as SliderChangedEventArgs)?.Value;
			}
		}
	}
}