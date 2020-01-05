using System;
using System.Reflection;
using System.Collections.Generic;

using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		// Attribute for creating options UI elements
		[AttributeUsage(AttributeTargets.Field)]
		public class FieldAttribute: Attribute, Config.IFieldAttribute
		{
			string label;

			public FieldAttribute(string _label = null)
			{
				label = _label;
			}

			public void process(object config, FieldInfo field)
			{																				$"Options.FieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				// adds choice labels to LanguageHandler, changing array in the process
				void _registerChoiceLabels(string[] labels)
				{
					for (int i = 0; i < labels.Length; i++)
						registerLabel($"{field.Name}_{i}", ref labels[i]);
				}

				if (mainConfig == null)
					mainConfig = config as Config;

				if (label != null)
					registerLabel(field.Name, ref label);
				else
					label = field.Name;

				Config.Field cfgField = new Config.Field(config, field);

				if (field.FieldType == typeof(bool))
				{
					add(new ToggleOption(cfgField, label));
				}
				else
				if (field.FieldType == typeof(UnityEngine.KeyCode))
				{
					add(new KeyBindOption(cfgField, label));
				}
				else
				if (field.FieldType.IsEnum) // add choice option for enum, works only with default enums (zero-based, increased by 1)
				{
					List<string> list = new List<string>();

					foreach (var e in Enum.GetValues(field.FieldType))
						list.Add(e.ToString());

					string[] choices = list.ToArray();
					_registerChoiceLabels(choices);

					add(new ChoiceOption(cfgField, label, choices));
				}
				else
				if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
				{
					// creating ChoiceOption if we also have choice attribute
					if (GetCustomAttribute(field, typeof(ChoiceAttribute)) is ChoiceAttribute choice && choice.choices.Length > 0)
					{
						_registerChoiceLabels(choice.choices);
						add(new ChoiceOption(cfgField, label, choice.choices, choice.values));
					}
					else // creating SliderOption if we also have bounds attribute
					if (GetCustomAttribute(field, typeof(Config.Field.BoundsAttribute)) is Config.Field.BoundsAttribute bounds && bounds.isBothBoundsSet())
					{
						add(new SliderOption(cfgField, label, bounds.min, bounds.max));
					}
					else
						$"Options.FieldAttribute: '{field.Name}' For numeric option field you also need to add ChoiceAttribute or FieldBoundsAttribute".logError();
				}
				else
					$"Options.FieldAttribute: '{field.Name}' Unsupported field type".logError();
			}
		}
	}
}