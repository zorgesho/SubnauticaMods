using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Configuration
{
	partial class Options
	{
		static class Factory
		{
			interface ICreator
			{
				ModOption create(Config.Field cfgField);
			}
			interface IModifier
			{
				void process(ModOption option);
			}

			static readonly List<ICreator>  creators  = _getList<ICreator>();
			static readonly List<IModifier> modifiers = _getList<IModifier>();

			static List<I> _getList<I>() => typeof(Factory).GetNestedTypes(ReflectionHelper.bfAll).
															Where(type => !type.IsInterface && typeof(I).IsAssignableFrom(type)).
															Select(Activator.CreateInstance).Cast<I>().
															ToList();

			// create mod option based on underlying type and attributes of cfgField
			public static ModOption create(Config.Field cfgField)
			{
				ModOption option = null;

				foreach (var c in creators)
					if ((option = c.create(cfgField)) != null)
						break;

				if (option != null)
					modifiers.ForEach(m => m.process(option));

				return option;
			}

			#region creators
			class ToggleOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type != typeof(bool))
						return null;

					return new ToggleOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label);
				}
			}

			class KeyBindOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type != typeof(UnityEngine.KeyCode))
						return null;

					return new KeyBindOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label);
				}
			}

			class ChoiceOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type.IsEnum) // add choice option for enum
					{
						var values = new List<object>();
						foreach (var val in Enum.GetValues(cfgField.type))
							values.Add(val.toInt());

						return new ChoiceOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label, Enum.GetNames(cfgField.type), values.ToArray());
					}

					if (cfgField.type == typeof(float) || cfgField.type == typeof(int)) // creating ChoiceOption if we also have choice attribute
					{
						if (cfgField.getAttr<ChoiceAttribute>() is ChoiceAttribute choice && choice.choices.Length > 0)
							return new ChoiceOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label, choice.choices, choice.values);
					}

					return null;
				}
			}

			class SliderOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					// creating SliderOption if we have range for the field (from RangeAttribute or SliderAttribute)
					if (cfgField.type != typeof(float) && cfgField.type != typeof(int))
						return null;

					var rangeAttr  = cfgField.getAttr<Config.Field.RangeAttribute>();
					var sliderAttr = cfgField.getAttr<SliderAttribute>();

					// slider range can't be wider than field range
					float min = Math.Max(rangeAttr?.min ?? float.MinValue, sliderAttr?.minValue ?? float.MinValue);
					float max = Math.Min(rangeAttr?.max ?? float.MaxValue, sliderAttr?.maxValue ?? float.MaxValue);

					if (min == float.MinValue || max == float.MaxValue) // we need to have both bounds for creating slider
						return null;

					// in case of custom value type we add valueFormat in that component instead of SliderOption
					string valueFormat = sliderAttr?.customValueType == null? sliderAttr?.valueFormat: null;

					string label = cfgField.getAttr<FieldAttribute>()?.label;
					ModOption option = new SliderOption(cfgField, label, min, max, sliderAttr?.defaultValue, valueFormat);

					if (sliderAttr?.customValueType != null)
						option.addHandler(new Components.SliderValue.Add(sliderAttr.customValueType, sliderAttr.valueFormat));

					return option;
				}
			}
			#endregion

			#region modifiers
			class TooltipModifier: IModifier
			{
				public void process(ModOption option)
				{
					if (option.cfgField.getAttr<FieldAttribute>() is FieldAttribute fieldAttr)
					{
						if (fieldAttr.tooltipType != null || fieldAttr.tooltip != null)
							option.addHandler(new Components.Tooltip.Add(fieldAttr.tooltipType, fieldAttr.tooltip));
					}
				}
			}

			class HideableModifier: IModifier
			{
				public void process(ModOption option)
				{
					if (option.cfgField.getAttr<HideableAttribute>() is HideableAttribute hideableAttr)
						option.addHandler(new Components.Hider.Add(hideableAttr.visChecker, hideableAttr.groupID));
				}
			}
			#endregion
		}
	}
}