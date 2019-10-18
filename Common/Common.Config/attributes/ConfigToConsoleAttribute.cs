using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Common.Config
{
	// for use in other mods
	static public class ExportedCfgVarFields
	{
		static readonly List<string> fields = new List<string>();

		static public List<string> getFields() => fields;

		static internal void addField(string fieldName) => fields.Add(fieldName.ToLower());
	}
	
	partial class Config
	{
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
		public class AddToConsoleAttribute: Attribute, IConfigAttribute, IFieldAttribute
		{
			readonly string cfgNamespace = null;

			public AddToConsoleAttribute(string _cfgNamespace = null) => cfgNamespace = _cfgNamespace;

			public void process(object config)
			{
				FieldInfo[] fields = config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				foreach (var field in fields)
					process(config, field);
			}

			public void process(object config, FieldInfo field)
			{
				if (!ConfigVarsConsoleCommand.isInited && config is Config)
					ConfigVarsConsoleCommand.init(config as Config, cfgNamespace);

				if (field.FieldType.IsPrimitive)
				{
					FieldBoundsAttribute bounds = GetCustomAttribute(field, typeof(FieldBoundsAttribute)) as FieldBoundsAttribute;
					FieldCustomActionAttribute action = GetCustomAttribute(field, typeof(FieldCustomActionAttribute)) as FieldCustomActionAttribute;
					
					if (ConfigVarsConsoleCommand.addConfigField(new ConfigVarsConsoleCommand.ConfigField(config, field, action?.action, bounds)))
						ExportedCfgVarFields.addField((cfgNamespace != null? cfgNamespace + ".": "") + field.Name);
				}
					
				if (field.FieldType.IsClass)
					process(field.GetValue(config));
			}
		}
	}

	// internal stuff, used by AddFieldsToConsoleAttribute
	partial class Config
	{
		static class ConfigVarsConsoleCommand
		{
			const string cmdName = "setcfgvar"; // used also in OnConsoleCommand_setcfgvar

			static GameObject consoleObject = null;
			static string cfgNamespace = null; // optional namespace for use in console in case of duplicate names
			static Config mainConfig = null;

			static public bool isInited { get => consoleObject != null; }

			public class ConfigField
			{
				readonly object config;
				public readonly FieldInfo field;
				readonly IFieldCustomAction action;
				readonly FieldBoundsAttribute bounds;

				public ConfigField(object _config, FieldInfo _field, IFieldCustomAction _action, FieldBoundsAttribute _bounds)
				{
					config = _config;
					field = _field;
					action = _action;
					bounds = _bounds;
				}

				public void setFieldValue(object value)
				{
					try
					{
						field.SetValue(config, Convert.ChangeType(value, field.FieldType));
#if !DEBUG
						bounds?.process(config, field);
#endif
						action?.fieldCustomAction();
					}
					catch (Exception e)
					{
						Log.msg(e);
					}
				}
			}

			static readonly Dictionary<string, ConfigField> cfgFields = new Dictionary<string, ConfigField>();

			static public void init(Config config, string _cfgNamespace = null)
			{
				if (consoleObject == null)
				{
					if (config != null)
					{
						consoleObject = PersistentConsoleCommands.createGameObject<SetCfgVarCommand>("ConfigConsoleCommands_" + Strings.modName);

						mainConfig = config;
						cfgNamespace = _cfgNamespace;
					}
					else
						"ConfigVarsConsoleCommand.init mainConfig is null !".logError();
				}
			}


			static public bool addConfigField(ConfigField configField)
			{																									$"ConfigVarsConsoleCommand: adding field {configField.field.Name}".logDbg();
				string name = configField.field.Name.ToLower();

				if (cfgFields.ContainsKey(name))
				{
					$"ConfigVarsConsoleCommand: field {name} is already added!".logError();
					return false;
				}
				else
				{
					cfgFields[name] = configField;
					return true;
				}
			}

			static void setFieldValue(string fieldName, string fieldValue)
			{
				try
				{
					if (cfgNamespace != null)
					{
						if (fieldName.StartsWith(cfgNamespace))
							fieldName = fieldName.Replace(cfgNamespace + ".", "");
						else
							return;
					}

					if (cfgFields.TryGetValue(fieldName, out ConfigField cf))
					{																							$"ConfigVarsConsoleCommand: field {fieldName} value {fieldValue}".logDbg();
						cf.setFieldValue(fieldValue);
						mainConfig.save();
					}
				}
				catch (Exception e)
				{
					Log.msg(e);
				}
			}

			class SetCfgVarCommand: PersistentConsoleCommands
			{
				void OnConsoleCommand_setcfgvar(NotificationCenter.Notification n)
				{
					if (n?.data != null && n.data.Count == 2)
					{																			$"setcfgvar raw: '{n.data[0]}' '{n.data[1]}'".logDbg();
						setFieldValue(n.data[0] as string, n.data[1] as string);
					}
				}
			}
		}
	}
}