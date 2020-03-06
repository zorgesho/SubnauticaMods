using System;
using System.Linq;
using System.Collections.Generic;

using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
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
					if (cfgField.type.IsEnum) // add choice option for enum, works only with default enums (zero-based, increased by 1)
					{
						var list = new List<string>();

						foreach (var e in Enum.GetValues(cfgField.type))
							list.Add(e.ToString());

						return new ChoiceOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label, list.ToArray());
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
					if (cfgField.type == typeof(float) || cfgField.type == typeof(int)) // creating SliderOption if we also have range attribute
					{
						if (cfgField.getAttr<Config.Field.RangeAttribute>() is Config.Field.RangeAttribute range && range.isBothBoundsSet())
						{
							SliderAttribute sliderAttr = cfgField.getAttr<SliderAttribute>();
							// in case of custom value type we add valueFormat in that component instead of SliderOption
							string valueFormat = sliderAttr?.customValueType == null? sliderAttr?.valueFormat: null;

							string label = cfgField.getAttr<FieldAttribute>()?.label;
							ModOption option = new SliderOption(cfgField, label, range.min, range.max, sliderAttr?.defaultValue, valueFormat);

							if (sliderAttr?.customValueType != null)
								option.addHandler(new Components.SliderValue.Add(sliderAttr.customValueType, sliderAttr.valueFormat));

							return option;
						}
					}

					return null;
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