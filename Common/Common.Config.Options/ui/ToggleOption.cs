using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public class ToggleOption: ModOption
		{
			public ToggleOption(Config.Field cfgField, string label): base(cfgField, label) {}

			public override void addOption(Options options)
			{
				options.AddToggleOption(id, label, cfgField.value.toBool());
			}

			public override void onEvent(EventArgs e)
			{
				cfgField.value = (e as ToggleChangedEventArgs)?.Value;
			}
		}
	}
}