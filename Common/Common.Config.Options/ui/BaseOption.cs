using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		abstract class ModOption
		{
			public struct InitParams
			{
				public object config;
				public FieldInfo field;
				public string label;
				public Config.IFieldCustomAction action;
			}

			public readonly string id;
			protected readonly string label;

			readonly object config;
			readonly FieldInfo field;
			readonly Config.IFieldCustomAction action;

			protected object fieldValue
			{
				get => field.GetValue(config);

				set
				{
					try
					{
						field.SetValue(config, Convert.ChangeType(value, field.FieldType));
					}
					catch (Exception e)
					{
						Log.msg(e);
					}
				}
			}

			public ModOption(InitParams p)
			{
				field = p.field;
				config = p.config;
				action = p.action;

				id = field.Name;
				label = p.label;
			}

			abstract public void addOption(Options options);

			virtual public void onEvent(EventArgs e)
			{
				mainConfig?.save();
				action?.fieldCustomAction();
			}
		}
	}
}