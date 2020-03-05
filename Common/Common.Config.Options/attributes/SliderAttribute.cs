using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		// used by FieldAttribute, don't do anything on its own
		[AttributeUsage(AttributeTargets.Field)]
		public class SliderAttribute: Attribute
		{
			public readonly Type customValueType; // component derived from ModSliderOption.SliderValue
			public readonly float? defaultValue;
			public readonly string valueFormat;

			public SliderAttribute(float DefaultValue = float.MinValue, string ValueFormat = null, Type CustomValueType = null)
			{
				defaultValue = (DefaultValue == float.MinValue)? (float?)null: DefaultValue; // can't use float? in attributes
				valueFormat = ValueFormat;
				customValueType = CustomValueType;

				Debug.assert(customValueType == null || typeof(ModSliderOption.SliderValue).IsAssignableFrom(customValueType),
					$"Custom value type {customValueType} is not derived from ModSliderOption.SliderValue");
			}
		}
	}
}