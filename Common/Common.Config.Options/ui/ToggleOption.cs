using System;
using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		class ToggleOption: ModOption
		{
			public ToggleOption(InitParams p): base(p) {}

			override public void addOption(Options options)
			{
				options.AddToggleOption(id, label, fieldValue.toBool());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as ToggleChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}
	}
}