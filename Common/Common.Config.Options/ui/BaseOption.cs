using System;
using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		abstract class ModOption
		{
			public readonly string id;
			protected readonly string label;

			protected readonly Config.CfgField cfgField;

			public ModOption(Config.CfgField cf, string _label)
			{
				cfgField = cf;
				id = cfgField.name;
				label = _label;
			}

			abstract public void addOption(Options options);

			virtual public void onEvent(EventArgs e)
			{
				mainConfig?.save();
			}
		}
	}
}