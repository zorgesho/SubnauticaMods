using System;

namespace Common.Configuration
{
	partial class Options
	{
		// Class attribute for setting mod options name in menu
		[AttributeUsage(AttributeTargets.Class)]
		public class NameAttribute: Attribute, Config.IConfigAttribute
		{
			string name;

			public NameAttribute(string _name) => name = _name;

			public void process(object config)
			{
				registerLabel("Name", ref name);
				optionsName = name;
			}
		}
	}
}