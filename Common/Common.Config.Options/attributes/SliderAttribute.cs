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

			public SliderAttribute(	float DefaultValue = float.MinValue,
									float MinValue = float.MinValue,
									float MaxValue = float.MaxValue,
									string ValueFormat = null,
									Type CustomValueType = null)
			{
				minValue = MinValue;
				maxValue = MaxValue;
				defaultValue = (DefaultValue == float.MinValue)? (float?)null: DefaultValue; // can't use float? in attributes

				valueFormat = ValueFormat;
				customValueType = CustomValueType;

				Debug.assert(customValueType == null || typeof(ModSliderOption.SliderValue).IsAssignableFrom(customValueType),
					$"Custom value type {customValueType} is not derived from ModSliderOption.SliderValue");
			}
		}
	}
}