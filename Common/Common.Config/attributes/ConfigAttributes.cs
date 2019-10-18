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
		public interface IConfigAttribute
		{
			void process(object config);
		}

		public interface IFieldAttribute
		{
			void process(object config, FieldInfo field);
		}

		void processAttributes() => processAttributes(this); // using static method because of possible nested config classes
		
		static void processAttributes(object config)
		{
			if (config == null)
				return;

			// processing attributes for config class
			Attribute[] configAttrs = Attribute.GetCustomAttributes(config.GetType());

			foreach (var attr in configAttrs)
				(attr as IConfigAttribute)?.process(config);
			
			// processing attributes for fields and nested classes
			FieldInfo[] fields = config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (FieldInfo field in fields)
			{																															$"Checking field '{field.Name}' for attributes".logDbg();
				Attribute[] attrs = Attribute.GetCustomAttributes(field);

				foreach (var attr in attrs)
					(attr as IFieldAttribute)?.process(config, field);

				if (field.FieldType.IsClass)
					processAttributes(field.GetValue(config));
			}
		}
	}
}