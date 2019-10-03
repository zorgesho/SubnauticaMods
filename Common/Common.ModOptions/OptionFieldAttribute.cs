using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	// Config attributes for Mod Options
	partial class Options: ModOptions
	{
		public interface IFieldCustomAction
		{
			void fieldCustomAction();
		}
		
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
			Type customActionType = null;

			public FieldAttribute(string Label = null, Type CustomActionType = null)
			{
				label = Label;
				customActionType = CustomActionType;
			}

			override public void process(object config, FieldInfo field)
			{																			$"OptionFieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				if (mainConfig == null && config is BaseConfig)
					mainConfig = config as BaseConfig;

				if (label == null)
					label = field.Name;

				ModOption.InitParams initParams = new ModOption.InitParams{config = config, field = field, label = label};

				if (customActionType != null)
				{
					initParams.action = Activator.CreateInstance(customActionType) as IFieldCustomAction;
					
					if (initParams.action == null)
						$"Options.FieldAttribute: '{field.Name}' You need to implement IFieldCustomAction in CustomAction".logError();
				}

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
					ChoiceAttribute choice = GetCustomAttribute(field, typeof(ChoiceAttribute)) as ChoiceAttribute;
					if (choice != null && choice.choices.Length > 0)
					{
						add(new ChoiceOption(initParams, choice.choices));
						return;
					}

					BoundsFieldAttribute bounds = GetCustomAttribute(field, typeof(BoundsFieldAttribute)) as BoundsFieldAttribute;
					if (bounds != null && bounds.isBothBoundsSet())
					{
						add(new SliderOption(initParams, bounds.min, bounds.max));
						return;
					}

					$"Options.FieldAttribute: '{field.Name}' For number option field you also need to add ChoiceAttribute or BoundsFieldAttribute".logError();
				}
				else
					$"Options.FieldAttribute: '{field.Name}' Unsupported field type".logError();
			}
		}
	}
}
