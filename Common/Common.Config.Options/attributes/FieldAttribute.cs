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

			readonly string label;
			readonly string tooltip;
			readonly Type tooltipType; // component derived from Options.Components.Tooltip

			public FieldAttribute(string Label = null, string Tooltip = null, Type TooltipType = null)
			{
				label = Label;
				tooltip = Tooltip;
				tooltipType = TooltipType;
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
			{																				$"Options.FieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				Config.Field cfgField = new Config.Field(config, field, rootConfig);

				ModOption option = null;
				if (field.FieldType == typeof(bool))
				{
					option = new ToggleOption(cfgField, label);
				}
				else
				if (field.FieldType == typeof(UnityEngine.KeyCode))
				{
					option = new KeyBindOption(cfgField, label);
				}
				else
				if (field.FieldType.IsEnum) // add choice option for enum, works only with default enums (zero-based, increased by 1)
				{
					List<string> list = new List<string>();

					foreach (var e in Enum.GetValues(field.FieldType))
						list.Add(e.ToString());

					option = new ChoiceOption(cfgField, label, list.ToArray());
				}
				else
				if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
				{
					// creating ChoiceOption if we also have choice attribute
					if (field.getAttribute<ChoiceAttribute>() is ChoiceAttribute choice && choice.choices.Length > 0)
					{
						option = new ChoiceOption(cfgField, label, choice.choices, choice.values);
					}
					else // creating SliderOption if we also have range attribute
					if (field.getAttribute<Config.Field.RangeAttribute>() is Config.Field.RangeAttribute range && range.isBothBoundsSet())
					{
						SliderAttribute sliderAttr = field.getAttribute<SliderAttribute>();
						// in case of custom value type we add valueFormat in that component instead of SliderOption
						string valueFormat = sliderAttr?.customValueType == null? sliderAttr?.valueFormat: null;

						option = new SliderOption(cfgField, label, range.min, range.max, sliderAttr?.defaultValue, valueFormat);

						if (sliderAttr?.customValueType != null)
							option.addHandler(new Components.SliderValue.Add(sliderAttr.customValueType, sliderAttr.valueFormat));
					}
					else
						$"Options.FieldAttribute: '{field.Name}' For numeric option field you also need to add ChoiceAttribute or RangeAttribute".logError();
				}
				else
					$"Options.FieldAttribute: '{field.Name}' Unsupported field type".logError();

				if (option != null)
				{
					if (tooltipType != null || tooltip != null)
						option.addHandler(new Components.Tooltip.Add(tooltipType, tooltip));

					if (field.getAttribute<HideableAttribute>() is HideableAttribute hideable)
						option.addHandler(new Components.Hider.Add(hideable.visChecker, hideable.groupID));

					add(option);
				}
			}
		}
	}
}