using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		class ChoiceOption: ModOption
		{
			readonly string[] choices = null;

			public ChoiceOption(Config.CfgField cfgField, string label, string[] _choices): base(cfgField, label)
			{
				choices = _choices;
			}

			override public void addOption(Options options)
			{
				options.AddChoiceOption(id, label, choices, cfgField.value.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				cfgField.value = (e as ChoiceChangedEventArgs)?.Index;
				base.onEvent(e);
			}
		}
	}
}