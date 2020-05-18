using System;
using System.Reflection;

namespace Common.Configuration
{
	using Reflection;

	partial class Config
	{
		public partial class Field
		{
			public readonly Config rootConfig;

			public readonly object parent;
			protected readonly FieldInfo field;

			protected readonly IAction[] actions;

			public Field(object _parent, FieldInfo _field, Config _rootConfig = null)
			{
				parent = _parent;
				field  = _field;
				Debug.assert(parent != null && field != null);

				rootConfig = _rootConfig ?? parent as Config ?? Config.main;
				Debug.assert(rootConfig != null, $"rootConfig is null (parent: '{parent?.GetType()}', field: '{field?.Name}')");
				Debug.assert(path != null,
					$"field path is null (rootConfig: {rootConfig.GetType()}, parent: '{parent?.GetType()}', field: '{field?.Name}')");

				var actionAttrs = getAttrs<ActionAttribute>(true);
				if (actionAttrs.Length > 0)
				{
					actions = new IAction[actionAttrs.Length];

					for (int i = 0; i < actions.Length; i++)
					{
						actions[i] = actionAttrs[i].action;
						(actions[i] as IRootConfigInfo)?.setRootConfig(rootConfig);
					}
				}
			}

			public Field(object parent, string fieldName, Config rootConfig = null):
				this(parent, parent?.GetType().field(fieldName), rootConfig) {}

			public Type type => field.FieldType;
			public string name => field.Name;

			public string path => _path ??= rootConfig.getFieldPath(parent, field);
			string _path = null;

			public A getAttr<A>(bool includeDeclaringTypes = false) where A: Attribute =>
				field.getAttr<A>(includeDeclaringTypes);

			public A[] getAttrs<A>(bool includeDeclaringTypes = false) where A: Attribute =>
				field.getAttrs<A>(includeDeclaringTypes);

			public bool checkAttr<A>() where A: Attribute => field.checkAttr<A>();

			public virtual object value
			{
				get => field.GetValue(parent);

				set
				{
					if (!parent.setFieldValue(field, value))
						return;

					actions?.forEach(a => a.action());
					rootConfig.save();
				}
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