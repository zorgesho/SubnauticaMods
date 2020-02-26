using System;
using System.Reflection;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class Field
		{
			[AttributeUsage(AttributeTargets.Field)]
			public class RangeAttribute: Attribute, IFieldAttribute
			{
				public readonly float min, max;

				public RangeAttribute(float Min = float.MinValue, float Max = float.MaxValue)
				{
					min = Min;
					max = Max;
				}

				public bool isBothBoundsSet() => min > float.MinValue && max < float.MaxValue;

				public object clamp(object value) => UnityEngine.Mathf.Clamp(value.toFloat(), min, max);

				public void process(object config, FieldInfo field)
				{																									$"RangeAttribute.process min > max, field '{field.Name}'".logDbgError(min > max);
					try
					{
						config.setFieldValue(field, clamp(field.GetValue(config)));
					}
					catch (Exception e)
					{
						Log.msg(e, $"config field {field.Name}");
					}
				}
			}
		}
	}
}