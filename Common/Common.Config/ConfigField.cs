using System;
using System.Reflection;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class Field
		{
			protected readonly Config rootConfig;

			protected readonly object parent;
			protected readonly FieldInfo field;

			protected readonly ICustomAction[] actions;

			public Field(object _parent, FieldInfo _field, Config _rootConfig = null)
			{
				parent = _parent;
				field  = _field;
				Debug.assert(parent != null && field != null);

				rootConfig = _rootConfig ?? parent as Config ?? Config.main;
				Debug.assert(rootConfig != null, "rootConfig is null");
				Debug.assert(path != null, "field path is null");

				CustomActionAttribute[] actionAttrs = field.getAttributes<CustomActionAttribute>();
				if (actionAttrs.Length > 0)
				{
					actions = new ICustomAction[actionAttrs.Length];

					for (int i = 0; i < actions.Length; i++)
						actions[i] = actionAttrs[i].action;
				}
			}

			public Field(object parent, string fieldName, Config rootConfig = null):
				this(parent, parent?.GetType().field(fieldName), rootConfig) {}

			public Type type => field.FieldType;
			public string name => field.Name;

			public string path => _path ?? (_path = rootConfig.getFieldPath(parent, field));
			string _path = null;

			public A getAttr<A>() where A: Attribute => field.getAttribute<A>();

			public object value
			{
				get => field.GetValue(parent);

				set => setFieldValue(value);
			}

			protected virtual void setFieldValue(object newValue)
			{
				if (value.Equals(newValue))
					return;

				parent.setFieldValue(field, newValue);
				actions?.forEach(action => action.customAction());

				rootConfig.save();
			}
		}
	}

	partial class Config
	{
		// dot-separated path to field from config's root
		string getFieldPath(object parent, FieldInfo fieldInfo)
		{
			return _getPath(this);

			string _getPath(object obj)
			{
				if (obj == null)
					return null;

				foreach (var field in obj.GetType().fields())
				{
					if (obj == parent && field == fieldInfo)
						return field.Name;

					if (_isInnerFieldsProcessable(field))
					{
						string path = _getPath(field.GetValue(obj));

						if (path != null)
							return field.Name + "." + path;
					}
				}

				return null;
			}
		}
	}
}