using System;
using System.Reflection;

namespace Common.Configuration
{
	using Reflection;

	partial class Config
	{
		public partial class Field
		{
			[AttributeUsage(AttributeTargets.Field)]
			public class RangeAttribute: Attribute, IFieldAttribute
			{
				public readonly float min, max;

				public RangeAttribute(float min = float.MinValue, float max = float.MaxValue)
				{
					this.min = min;
					this.max = max;
				}

				public object clamp(object value) => UnityEngine.Mathf.Clamp(value.convert<float>(), min, max);

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