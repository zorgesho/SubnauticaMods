using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options
	{
		// used by FieldAttribute, don't do anything on its own
		[AttributeUsage(AttributeTargets.Field)]
		public class SliderAttribute: Attribute
		{
			public readonly float? defaultValue;
			public readonly float  minValue, maxValue;

			public readonly Type customValueType; // component derived from ModSliderOption.SliderValue
			public readonly string valueFormat;

			public SliderAttribute(	float defaultValue = float.MinValue,
									float minValue = float.MinValue,
									float maxValue = float.MaxValue,
									string valueFormat = null,
									Type customValueType = null)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
				this.defaultValue = (defaultValue == float.MinValue)? (float?)null: defaultValue; // can't use float? in attributes

				this.valueFormat = valueFormat;
				this.customValueType = customValueType;

				Debug.assert(customValueType == null || typeof(ModSliderOption.SliderValue).IsAssignableFrom(customValueType),
					$"Custom value type {customValueType} is not derived from ModSliderOption.SliderValue");
			}
		}
	}
}