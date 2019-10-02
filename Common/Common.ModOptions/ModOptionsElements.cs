using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		abstract class ModOption
		{
			public readonly string id;
			protected readonly string label;

			readonly object config;
			readonly FieldInfo field;

			protected object fieldValue
			{
				get => field.GetValue(config);

				set
				{
					try
					{
						field.SetValue(config, Convert.ChangeType(value, field.FieldType));
					}
					catch (Exception e)
					{
						Log.msg(e);
					}
				}
			}

			public ModOption(object _config, FieldInfo _field, string _label)
			{
				field = _field;
				config = _config;

				id = field.Name;
				label = _label;
			}

			abstract public void addOption(Options options);

			virtual public void onEvent(EventArgs e)
			{
				mainConfig?.save();
				// custom field handling will go there if needed
			}
		}


		class ToggleOption: ModOption
		{
			public ToggleOption(object _config, FieldInfo _fieldInfo, string _label):
				base(_config, _fieldInfo, _label)
			{
			}

			override public void addOption(Options options)
			{
				options.AddToggleOption(id, label, fieldValue.toBool());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as ToggleChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}


		class SliderOption: ModOption
		{
			float min, max;

			public SliderOption(object _config, FieldInfo _fieldInfo, string _label, float _min, float _max):
				base(_config, _fieldInfo, _label)
			{
				min = _min;
				max = _max;
			}

			override public void addOption(Options options)
			{
				options.AddSliderOption(id, label, min, max, fieldValue.toFloat());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as SliderChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}


		class ChoiceOption: ModOption
		{
			string[] choices = null;

			public ChoiceOption(object _config, FieldInfo _fieldInfo, string _label, string[] _choices):
				base(_config, _fieldInfo, _label)
			{
				choices = _choices;
			}

			override public void addOption(Options options)
			{
				options.AddChoiceOption(id, label, choices, fieldValue.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as ChoiceChangedEventArgs)?.Index;
				base.onEvent(e);
			}
		}


		class KeyBindOption: ModOption
		{
			public KeyBindOption(object _config, FieldInfo _fieldInfo, string _label):
				base(_config, _fieldInfo, _label)
			{
			}

			override public void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, (UnityEngine.KeyCode)fieldValue.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as KeybindChangedEventArgs)?.Key;
				base.onEvent(e);
			}
		}
	}
}