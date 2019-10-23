using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace Common.Configuration
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
			readonly string cfgNamespace;

			public AddToConsoleAttribute(string _cfgNamespace = null) => cfgNamespace = _cfgNamespace;

			public void process(object config)
			{
				FieldInfo[] fields = config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				fields.forEach(field => process(config, field));
			}

			public void process(object config, FieldInfo field)
			{
				if (!ConfigVarsConsoleCommand.isInited && config is Config)
					ConfigVarsConsoleCommand.init(config as Config, cfgNamespace);

				if (field.FieldType.IsPrimitive)
					if (ConfigVarsConsoleCommand.addConfigField(new ConfigVarsConsoleCommand.CfgField(config, field)))
						ExportedCfgVarFields.addField((cfgNamespace != null? cfgNamespace + ".": "") + field.Name);
					
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
			static GameObject consoleObject = null;
			static string cfgNamespace = null; // optional namespace for use in console in case of duplicate names
			static Config mainConfig = null;

			static public bool isInited { get => consoleObject != null; }

			public class CfgField: Config.CfgField
			{
				readonly FieldBoundsAttribute bounds = null;

				public CfgField(object config, FieldInfo field): base(config, field)
				{
#if !DEBUG
					bounds = Attribute.GetCustomAttribute(field, typeof(FieldBoundsAttribute)) as FieldBoundsAttribute;
#endif
				}

				override protected void setFieldValue(object value)
				{
					base.setFieldValue(bounds != null? bounds.applyBounds(value): value);
				}
			}

			static readonly Dictionary<string, CfgField> cfgFields = new Dictionary<string, CfgField>();

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


			static public bool addConfigField(CfgField cfgField)
			{																									$"ConfigVarsConsoleCommand: adding field {cfgField.name}".logDbg();
				string name = cfgField.name.ToLower();

				if (cfgFields.ContainsKey(name))
				{
					$"ConfigVarsConsoleCommand: field {name} is already added!".logError();
					return false;
				}
				else
				{
					cfgFields[name] = cfgField;
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

					if (cfgFields.TryGetValue(fieldName, out CfgField cf))
					{																							$"ConfigVarsConsoleCommand: field {fieldName} value {fieldValue}".logDbg();
						cf.value = fieldValue;
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