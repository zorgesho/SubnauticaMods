using System;

using UnityEngine;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public class SliderOption: ModOption
		{
			public class OptionalProps
			{
				public readonly Type customValue; // ModSliderOption.SliderValue component
				public readonly float? defaultValue;
				public readonly string valueFormat;

				public OptionalProps(float? _defaultValue, string _valueFormat, Type _customValue)
				{
					defaultValue = _defaultValue;
					valueFormat  = _valueFormat;
					customValue  = _customValue;

					Debug.assert(customValue == null || typeof(ModSliderOption.SliderValue).IsAssignableFrom(customValue), "CustomValue should be derived from ModSliderOption.SliderValue");
				}
			}

			readonly float min, max;
			public OptionalProps optionalProps;

			public SliderOption(Config.Field cfgField, string label, float _min, float _max): this(cfgField, label, null, _min, _max) {}

			public SliderOption(Config.Field cfgField, string label, string tooltip, float _min, float _max): base(cfgField, label, tooltip)
			{
				min = _min;
				max = _max;
			}

			public override void addOption(Options options)
			{
				string valueFormat = optionalProps?.customValue != null? null: optionalProps?.valueFormat; // in case of custom value component set valueFormat there
				options.AddSliderOption(id, label, min, max, cfgField.value.toFloat(), optionalProps?.defaultValue, valueFormat);
			}

			public override void onChangeValue(EventArgs e)
			{
				cfgField.value = (e as SliderChangedEventArgs)?.Value;
			}

			public override void onChangeGameObject(GameObject go)
			{
				base.onChangeGameObject(go);

				if (optionalProps.customValue == null)
					return;

				GameObject slider = gameObject.transform.Find("Slider").gameObject;
				ModSliderOption.SliderValue sliderValue = slider.AddComponent(optionalProps.customValue) as ModSliderOption.SliderValue;
				Debug.assert(sliderValue != null);

				sliderValue.ValueFormat = optionalProps.valueFormat;
			}
		}
	}
}