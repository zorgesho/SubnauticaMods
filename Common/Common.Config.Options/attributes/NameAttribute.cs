using System;

namespace Common.Configuration
{
	partial class Options
	{
		// Class attribute for setting mod options name and tooltip in menu
		[AttributeUsage(AttributeTargets.Class)]
		public class NameAttribute: Attribute, Config.IConfigAttribute
		{
			string name;
			public readonly string tooltip;
			public readonly Type tooltipType; // component derived from Options.Components.Tooltip

			public NameAttribute(string name, string tooltip = null, Type tooltipType = null)
			{
				this.name = name;
				this.tooltip = tooltip;
				this.tooltipType = tooltipType;
			}

			public void process(object config)
			{
				registerLabel("Name", ref name);
				optionsName = name;
			}
		}
	}
}