using System;
using System.Reflection;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class CfgField
		{
			protected readonly object config;
			protected readonly FieldInfo field;

			protected readonly ICustomAction action;

			public CfgField(object _config, FieldInfo _field)
			{
				config = _config;
				field = _field;

				action = (Attribute.GetCustomAttribute(field, typeof(CustomActionAttribute)) as CustomActionAttribute)?.action;
			}

			public string name
			{
				get => field.Name;
			}

			public object value
			{
				get => field.GetValue(config);

				set => setFieldValue(value);
			}

			virtual protected void setFieldValue(object value)
			{
				config.setFieldValue(field, value);
				action?.customAction();
			}
		}
	}
}