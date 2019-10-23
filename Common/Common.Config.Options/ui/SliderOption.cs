using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		class SliderOption: ModOption
		{
			readonly float min, max;

			public SliderOption(Config.CfgField cfgField, string label, float _min, float _max): base(cfgField, label)
			{
				min = _min;
				max = _max;
			}

			override public void addOption(Options options)
			{
				options.AddSliderOption(id, label, min, max, cfgField.value.toFloat());
			}

			override public void onEvent(EventArgs e)
			{
				cfgField.value = (e as SliderChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}
	}
}