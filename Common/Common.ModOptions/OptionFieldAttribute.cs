using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		[AttributeUsage(AttributeTargets.Class)]
		public class NameAttribute: ConfigAttribute
		{
			string optionsName;

			public NameAttribute(string name)
			{
				optionsName = name;
			}

			override public void process(object config)
			{
				name = optionsName;
				mainConfig = config as BaseConfig;
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class ChoiceAttribute: ConfigFieldAttribute
		{
			public readonly string[] choices = null;

			public ChoiceAttribute(params string[] choices_)
			{
				choices = choices_;
			}

			override public void process(object config, FieldInfo field)
			{
				$"CHOICE ATTR PROCESS".logDbg();
			}
		}


		[AttributeUsage(AttributeTargets.Field)]
		public class FieldAttribute: ConfigFieldAttribute
		{
			string label = null;

			public FieldAttribute(string _label = null)
			{
				label = _label;
			}

			override public void process(object config, FieldInfo field)
			{																			$"OptionFieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				if (mainConfig == null && config is BaseConfig)
					mainConfig = config as BaseConfig;

				if (label == null)
					label = field.Name;

				if (field.FieldType == typeof(bool))
				{
					add(new ToggleOption(config, field, label));
				}
				else
				if (field.FieldType == typeof(UnityEngine.KeyCode))
				{
					add(new KeyBindOption(config, field, label));
				}
				else
				if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
				{
					ChoiceAttribute choice = GetCustomAttribute(field, typeof(ChoiceAttribute)) as ChoiceAttribute;
					if (choice != null && choice.choices.Length > 0)
					{
						add(new ChoiceOption(config, field, label, choice.choices));
						return;
					}

					BoundsFieldAttribute bounds = GetCustomAttribute(field, typeof(BoundsFieldAttribute)) as BoundsFieldAttribute;
					if (bounds != null && bounds.isBothBoundsSet())
					{
						add(new SliderOption(config, field, label, bounds.min, bounds.max));
						return;
					}

					$"You need set bounds or something!!!!".logError();
				}
				else
					$"UNSUPPORTED !!!!!!!".logError();
			}
		}
	}
}
