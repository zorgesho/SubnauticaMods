using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		// Class attribute for setting mod options name in menu
		[AttributeUsage(AttributeTargets.Class)]
		public class NameAttribute: Attribute, Config.IConfigAttribute
		{
			string optionsName;

			public NameAttribute(string name)
			{
				optionsName = name;
			}

			public void process(object config)
			{
				registerLabel("Name", ref optionsName);
				name = optionsName;
				mainConfig = config as Config;
			}
		}
	}
}