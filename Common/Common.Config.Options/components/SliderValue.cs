using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public static partial class Components
		{
			public static class SliderValue
			{
				public class Add: ModOption.IOnGameObjectChangeHandler
				{
					readonly Type valueCmpType;
					readonly string valueFormat;

					public void init(ModOption option)
					{
						Debug.assert(option is SliderOption, "SliderValue.Add: option is not a slider");
					}

					public Add(Type _valueCmpType, string _valueFormat)
					{
						valueCmpType = _valueCmpType;
						valueFormat  = _valueFormat;

						Debug.assert(typeof(ModSliderOption.SliderValue).IsAssignableFrom(valueCmpType),
							$"Custom value type {valueCmpType} is not derived from ModSliderOption.SliderValue");
					}

					public void handle(GameObject gameObject)
					{
						GameObject slider = gameObject.transform.Find("Slider").gameObject;
						(slider.AddComponent(valueCmpType) as ModSliderOption.SliderValue).ValueFormat = valueFormat;
					}
				}


				// breaks slider to several linear intervals
				public class Nonlinear: ModSliderOption.SliderValue
				{
					readonly List<Tuple<float, float>> sliderToDisplay = new List<Tuple<float, float>>();
					readonly List<Tuple<float, float>> displayToSlider = new List<Tuple<float, float>>();

					// Item1: sliderValue; Item2: formatBefore; Item3: formatAfter
					readonly List<Tuple<float, string, string>> valueFormats = new List<Tuple<float, string, string>>();

					public override float ValueWidth => 0f;

					protected override IEnumerator Start() => null;

					protected override void InitConverters()
					{
						addValueInfo(0f, minValue);
						addValueInfo(1f, maxValue);

						sliderToDisplay.Sort((x, y) => Math.Sign(x.Item1 - y.Item1));
						displayToSlider.Sort((x, y) => Math.Sign(x.Item1 - y.Item1));
						valueFormats.Sort((x, y) => Math.Sign(x.Item1 - y.Item1));
					}

					public override float ConvertToSliderValue(float value)
					{
						return convertValue(value, displayToSlider);
					}

					public override float ConvertToDisplayValue(float value)
					{
						int index = valueFormats.FindIndex(1, info => value <= info.Item1);
						if (index > 0)
							ValueFormat = valueFormats[index].Item2 ?? valueFormats[index - 1].Item3 ?? ValueFormat;

						return convertValue(value, sliderToDisplay);
					}

					float convertValue(float value, List<Tuple<float, float>> valueInfo)
					{
						float _convert(float fromLeft, float fromRight, float toLeft, float toRight) =>
							toLeft + (toRight - toLeft) / (fromRight - fromLeft) * (value - fromLeft); // zero ?

						int index = valueInfo.FindIndex(1, info => value <= info.Item1);
						return index < 1? value: _convert(valueInfo[index-1].Item1, valueInfo[index].Item1, valueInfo[index-1].Item2, valueInfo[index].Item2);
					}

					// conversion point, sliderValue <-> displayValue
					// formatBefore: value format for items lesser than sliderValue; formatAfter: for items greater than sliderValue
					protected void addValueInfo(float sliderValue, float displayValue, string formatBefore = null, string formatAfter = null)
					{
						sliderToDisplay.Add(Tuple.Create(sliderValue, displayValue));
						displayToSlider.Add(Tuple.Create(displayValue, sliderValue));

						valueFormats.Add(Tuple.Create(sliderValue, formatBefore, formatAfter));
					}
				}
			}
		}
	}
}