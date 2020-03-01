using System;
using System.Reflection;
using System.Collections.Generic;

using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		// Attribute for creating options UI elements
		// AttributeTargets.Class is just for convenience during development (try to create options UI elements for all inner fields)
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
		public class FieldAttribute: Attribute, Config.IConfigAttribute, Config.IFieldAttribute, Config.IRootConfigInfo
		{
			Config rootConfig;
			public void setRootConfig(Config config) => rootConfig = config;

			string label;
			string tooltip;

			public FieldAttribute(string _label = null, string _tooltip = null)
			{
				label = _label;
				tooltip = _tooltip;
			}

			public void process(object config)
			{
				foreach (var field in config.GetType().fields())
				{
					process(config, field);
					label = tooltip = null;

					if (Config._isInnerFieldsProcessable(field))
						process(field.GetValue(config));
				}
			}

			public void process(object config, FieldInfo field)
			{																				$"Options.FieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				Config.Field cfgField = new Config.Field(config, field, rootConfig);

				if (field.FieldType == typeof(bool))
				{
					add(new ToggleOption(cfgField, label, tooltip));
				}
				else
				if (field.FieldType == typeof(UnityEngine.KeyCode))
				{
					add(new KeyBindOption(cfgField, label, tooltip));
				}
				else
				if (field.FieldType.IsEnum) // add choice option for enum, works only with default enums (zero-based, increased by 1)
				{
					List<string> list = new List<string>();

					foreach (var e in Enum.GetValues(field.FieldType))
						list.Add(e.ToString());

					add(new ChoiceOption(cfgField, label, tooltip, list.ToArray()));
				}
				else
				if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
				{
					// creating ChoiceOption if we also have choice attribute
					if (field.getAttribute<ChoiceAttribute>() is ChoiceAttribute choice && choice.choices.Length > 0)
					{
						add(new ChoiceOption(cfgField, label, tooltip, choice.choices, choice.values));
					}
					else // creating SliderOption if we also have range attribute
					if (field.getAttribute<Config.Field.RangeAttribute>() is Config.Field.RangeAttribute range && range.isBothBoundsSet())
					{
						add(new SliderOption(cfgField, label, tooltip, range.min, range.max));
					}
					else
						$"Options.FieldAttribute: '{field.Name}' For numeric option field you also need to add ChoiceAttribute or RangeAttribute".logError();
				}
				else
					$"Options.FieldAttribute: '{field.Name}' Unsupported field type".logError();
			}
		}
	}
}