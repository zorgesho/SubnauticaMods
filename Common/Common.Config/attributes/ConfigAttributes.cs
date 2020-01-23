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

		// for use with non-primitive types, inner fields will not be searched for attributes
		// (other attributes of the field will still be processed)
		[AttributeUsage(AttributeTargets.Field)]
		public class SkipRecursiveAttrProcessing: Attribute {}

		void processAttributes() => processAttributes(this); // using static method because of possible nested config classes

		static void processAttributes(object config)
		{
			if (config == null)
				return;

			// processing attributes for config class
			Attribute.GetCustomAttributes(config.GetType()).forEach(attr => (attr as IConfigAttribute)?.process(config));

			// processing attributes for fields and nested classes (don't process static fields)
			foreach (FieldInfo field in config.GetType().fields())
			{																															$"Checking field '{field.Name}' for attributes".logDbg();
				Attribute.GetCustomAttributes(field).forEach(attr => (attr as IFieldAttribute)?.process(config, field));

				if (!field.IsStatic && field.FieldType.IsClass && Attribute.GetCustomAttribute(field, typeof(SkipRecursiveAttrProcessing)) == null)
					processAttributes(field.GetValue(config));
			}
		}
	}
}