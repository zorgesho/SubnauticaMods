using System;
using System.Reflection;

namespace Common.Configuration
{
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
			Attribute.GetCustomAttributes(config.GetType()).forEach(attr => (attr as IConfigAttribute)?.process(config));

			// processing attributes for fields and nested classes
			FieldInfo[] fields = config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (FieldInfo field in fields)
			{																															$"Checking field '{field.Name}' for attributes".logDbg();
				Attribute.GetCustomAttributes(field).forEach(attr => (attr as IFieldAttribute)?.process(config, field));

				if (field.FieldType.IsClass)
					processAttributes(field.GetValue(config));
			}
		}
	}
}