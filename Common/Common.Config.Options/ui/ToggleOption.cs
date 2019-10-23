using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		class ToggleOption: ModOption
		{
			public ToggleOption(Config.Field cfgField, string label): base(cfgField, label) {}

			override public void addOption(Options options)
			{
				options.AddToggleOption(id, label, cfgField.value.toBool());
			}

			override public void onEvent(EventArgs e)
			{
				cfgField.value = (e as ToggleChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}
	}
}