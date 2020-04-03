using System;
using System.Reflection;

namespace Common.Configuration
{
	partial class Options
	{
		// Attribute for creating options UI elements
		// AttributeTargets.Class is just for convenience during development (try to create options UI elements for all inner fields)
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
		public class FieldAttribute: Attribute, Config.IConfigAttribute, Config.IFieldAttribute, Config.IRootConfigInfo
		{
			Config rootConfig;
			public void setRootConfig(Config config) => rootConfig = config;

			public readonly string label;
			public readonly string tooltip;
			public readonly Type tooltipType; // component derived from Options.Components.Tooltip

			public FieldAttribute(string label = null, string tooltip = null, Type tooltipType = null)
			{
				this.label = label;
				this.tooltip = tooltip;
				this.tooltipType = tooltipType;
			}

			public void process(object config)
			{
				foreach (var field in config.GetType().fields())
				{
					process(config, field);

					if (Config._isInnerFieldsProcessable(field))
						process(field.GetValue(config));
				}
			}

			public void process(object config, FieldInfo field)
			{																			$"Options.FieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				var cfgField = new Config.Field(config, field, rootConfig);

				if (Factory.create(cfgField) is ModOption option)
					add(option);
				else
					$"FieldAttribute.process: error while creating option for field {field.Name}".logError();
			}
		}
	}
}