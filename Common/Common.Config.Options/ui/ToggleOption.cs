using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public class ToggleOption: ModOption
		{
			public ToggleOption(Config.Field cfgField, string label, string tooltip = null): base(cfgField, label, tooltip) {}

			public override void addOption(Options options)
			{
				options.AddToggleOption(id, label, cfgField.value.toBool());
			}

			public override void onChangeValue(EventArgs e)
			{
				cfgField.value = (e as ToggleChangedEventArgs)?.Value;
			}
		}
	}
}