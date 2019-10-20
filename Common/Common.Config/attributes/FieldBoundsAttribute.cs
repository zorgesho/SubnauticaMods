using System;
using System.Reflection;

namespace Common.Config
{
	partial class Config
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class FieldBoundsAttribute: Attribute, IFieldAttribute
		{
			public readonly float min = float.MinValue;
			public readonly float max = float.MaxValue;

			public FieldBoundsAttribute(float Min = float.MinValue, float Max = float.MaxValue)
			{
				min = Min;
				max = Max;
			}

			public bool isBothBoundsSet() => min > float.MinValue && max < float.MaxValue;

			public void process(object config, FieldInfo field)
			{																					$"BoundsFieldAttribute.process min > max, field '{field.Name}'".logDbgError(min > max);
				try
				{
					float value = field.GetValue(config).toFloat();
					float valuePrev = value;

					value = UnityEngine.Mathf.Clamp(value, min, max);

					if (value != valuePrev)
					{																			$"BoundsFieldAttribute.process changing field '{field.Name}' from {valuePrev} to {value}".logWarning();
						config.setFieldValue(field, value);
					}
				}
				catch (Exception e)
				{
					Log.msg(e, $"config field {field.Name}");
				}
			}
		}
	}
}