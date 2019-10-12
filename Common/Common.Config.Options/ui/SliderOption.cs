using System;
using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		class SliderOption: ModOption
		{
			readonly float min, max;

			public SliderOption(InitParams p, float _min, float _max): base(p)
			{
				min = _min;
				max = _max;
			}

			override public void addOption(Options options)
			{
				options.AddSliderOption(id, label, min, max, fieldValue.toFloat());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as SliderChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}
	}
}