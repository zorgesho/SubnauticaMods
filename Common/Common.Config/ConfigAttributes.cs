using System;
using System.Reflection;

namespace Common.Config
{
	static class ConvertObjectExtensions
	{
		static public int toInt(this object obj) => Convert.ToInt32(obj);
		static public bool toBool(this object obj) => Convert.ToBoolean(obj);
		static public float toFloat(this object obj) => Convert.ToSingle(obj);
	}

	partial class Config
	{
		abstract public class ConfigAttribute: Attribute
		{
			abstract public void process(object config);
		}

		abstract public class FieldAttribute: Attribute
		{
			abstract public void process(object config, FieldInfo field);
		}

		
		void processAttributes() => processAttributes(this); // using static method because of possible nested config classes
		
		static void processAttributes(object config)
		{
			if (config == null)
				return;

			// processing attributes for config class
			ConfigAttribute[] configAttrs = Attribute.GetCustomAttributes(config.GetType(), typeof(ConfigAttribute)) as ConfigAttribute[];

			foreach (var attr in configAttrs)
				attr.process(config);
			
			// processing attributes for fields and nested classes
			FieldInfo[] fields = config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (FieldInfo field in fields)
			{																															$"Checking field '{field.Name}' for attributes".logDbg();
				if (Attribute.IsDefined(field, typeof(FieldAttribute)))
				{
					FieldAttribute[] attrs = Attribute.GetCustomAttributes(field, typeof(FieldAttribute)) as FieldAttribute[];

					foreach (var attr in attrs)
						attr.process(config, field);
				}

				if (field.FieldType.IsClass)
					processAttributes(field.GetValue(config));
			}
		}
	}
}