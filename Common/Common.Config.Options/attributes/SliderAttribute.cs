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
			public SliderOption.OptionalProps optionalProps;

			public SliderAttribute(float DefaultValue = float.MinValue, string ValueFormat = null, Type CustomValue = null)
			{
				float? defaultValue = (DefaultValue == float.MinValue)? (float?)null: DefaultValue; // can't use float? in attributes

				optionalProps = new SliderOption.OptionalProps(defaultValue, ValueFormat, CustomValue);
			}
		}
	}
}