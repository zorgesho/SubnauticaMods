using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		class ChoiceOption: ModOption
		{
			readonly string[] choices = null;
			readonly object[] values = null;

			public ChoiceOption(Config.Field cfgField, string label, string[] _choices, object[] _values = null): base(cfgField, label)
			{
				choices = _choices;
				values = _values;
			}

			public override void addOption(Options options)
			{
				int defaultIndex = (values != null)? values.findIndex(val => val.Equals(cfgField.value)): cfgField.value.toInt();
				options.AddChoiceOption(id, label, choices, defaultIndex < 0? 0: defaultIndex);
			}

			public override void onEvent(EventArgs e)
			{
				int? index = (e as ChoiceChangedEventArgs)?.Index;
				cfgField.value = (values != null)? values[index ?? 0]: index;
				base.onEvent(e);
			}
		}
	}
}