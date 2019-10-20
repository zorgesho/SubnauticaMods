﻿using System.Reflection;

namespace Common.Config
{
	partial class Config
	{
		public class CfgField
		{
			readonly object config;
			readonly FieldInfo field;
			readonly IFieldCustomAction action;

			public CfgField(object _config, FieldInfo _field, IFieldCustomAction _action = null)
			{
				config = _config;
				field = _field;
				action = _action;
			}

			public string name
			{
				get => field.Name;
			}

			public object value
			{
				get => field.GetValue(config);

				set 
				{
					config.setFieldValue(field, value);
					action?.fieldCustomAction();
				}
			}
		}
	}
}