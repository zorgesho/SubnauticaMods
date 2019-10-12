using System;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		// Class attribute for setting mod options name in menu
		[AttributeUsage(AttributeTargets.Class)]
		public class NameAttribute: Config.ConfigAttribute
		{
			readonly string optionsName;

			public NameAttribute(string name)
			{
				optionsName = name;
			}

			override public void process(object config)
			{
				name = optionsName;
				mainConfig = config as Config;
			}
		}
	}
}