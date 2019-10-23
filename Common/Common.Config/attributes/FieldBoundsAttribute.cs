using System;
using System.Reflection;

namespace Common.Configuration
{
	partial class Config
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class FieldBoundsAttribute: Attribute, IFieldAttribute
		{
			public readonly float min, max;

			public FieldBoundsAttribute(float Min = float.MinValue, float Max = float.MaxValue)
			{
				min = Min;
				max = Max;
			}

			public bool isBothBoundsSet() => min > float.MinValue && max < float.MaxValue;

			public object applyBounds(object value) => UnityEngine.Mathf.Clamp(value.toFloat(), min, max);

			public void process(object config, FieldInfo field)
			{																					$"BoundsFieldAttribute.process min > max, field '{field.Name}'".logDbgError(min > max);
				try
				{
					config.setFieldValue(field, applyBounds(field.GetValue(config)));
				}
				catch (Exception e)
				{
					Log.msg(e, $"config field {field.Name}");
				}
			}
		}
	}
}