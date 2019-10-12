using System;
using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		class ChoiceOption: ModOption
		{
			readonly string[] choices = null;

			public ChoiceOption(InitParams p, string[] _choices) : base(p)
			{
				choices = _choices;
			}

			override public void addOption(Options options)
			{
				options.AddChoiceOption(id, label, choices, fieldValue.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as ChoiceChangedEventArgs)?.Index;
				base.onEvent(e);
			}
		}
	}
}