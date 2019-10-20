using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		// Attribute for creating options UI elements
		[AttributeUsage(AttributeTargets.Field)]
		public class FieldAttribute: Attribute, Config.IFieldAttribute
		{
			string label = null;

			public FieldAttribute(string _label = null)
			{
				label = _label;
			}

			public void process(object config, FieldInfo field)
			{																			$"Options.FieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				if (mainConfig == null)
					mainConfig = config as Config;

				if (label == null)
					label = field.Name;

				Config.IFieldCustomAction action = (GetCustomAttribute(field, typeof(Config.FieldCustomActionAttribute)) as Config.FieldCustomActionAttribute)?.action;

				Config.CfgField cfgField = new Config.CfgField(config, field, action);

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
				if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
				{
					// creating ChoiceOption if we also have choice attribute
					ChoiceAttribute choice = GetCustomAttribute(field, typeof(ChoiceAttribute)) as ChoiceAttribute;
					if (choice != null && choice.choices.Length > 0)
					{
						add(new ChoiceOption(cfgField, label, choice.choices));
						return;
					}

					// creating SliderOption if we also have bounds attribute
					Config.FieldBoundsAttribute bounds = GetCustomAttribute(field, typeof(Config.FieldBoundsAttribute)) as Config.FieldBoundsAttribute;
					if (bounds != null && bounds.isBothBoundsSet())
					{
						add(new SliderOption(cfgField, label, bounds.min, bounds.max));
						return;
					}

					$"Options.FieldAttribute: '{field.Name}' For numeric option field you also need to add ChoiceAttribute or FieldBoundsAttribute".logError();
				}
				else
					$"Options.FieldAttribute: '{field.Name}' Unsupported field type".logError();
			}
		}
	}
}