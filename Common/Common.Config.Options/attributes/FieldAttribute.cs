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

				ModOption.InitParams initParams = new ModOption.InitParams{config = config, field = field, label = label};

				if (GetCustomAttribute(field, typeof(Config.FieldCustomActionAttribute)) is Config.FieldCustomActionAttribute action)
					initParams.action = action.action;

				if (field.FieldType == typeof(bool))
				{
					add(new ToggleOption(initParams));
				}
				else
				if (field.FieldType == typeof(UnityEngine.KeyCode))
				{
					add(new KeyBindOption(initParams));
				}
				else
				if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
				{
					// creating ChoiceOption if we also have choice attribute
					ChoiceAttribute choice = GetCustomAttribute(field, typeof(ChoiceAttribute)) as ChoiceAttribute;
					if (choice != null && choice.choices.Length > 0)
					{
						add(new ChoiceOption(initParams, choice.choices));
						return;
					}

					// creating SliderOption if we also have bounds attribute
					Config.FieldBoundsAttribute bounds = GetCustomAttribute(field, typeof(Config.FieldBoundsAttribute)) as Config.FieldBoundsAttribute;
					if (bounds != null && bounds.isBothBoundsSet())
					{
						add(new SliderOption(initParams, bounds.min, bounds.max));
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