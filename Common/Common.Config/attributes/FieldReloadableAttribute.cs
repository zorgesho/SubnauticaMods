using System;
using System.Reflection;

namespace Common.Configuration
{
	using Utils;

	partial class Config
	{
		public partial class Field
		{
			// field with this attribute will be reloaded if config changed outside of the game
			// value will change when application gets focus back
			[AttributeUsage(AttributeTargets.Field)]
			public class ReloadableAttribute: Attribute, IFieldAttribute, IRootConfigInfo
			{
				Config rootConfig;
				public void setRootConfig(Config config) => rootConfig = config;

				public void process(object config, FieldInfo field)
				{
					ConfigReloader.addField(new FieldRanged(config, field, rootConfig));
				}
			}
		}
	}
}