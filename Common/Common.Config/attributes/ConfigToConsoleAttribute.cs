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

			#region Internal stuff
			static class ConfigVarsConsoleCommand
			{
				static GameObject consoleObject = null;
				static string cfgNamespace = null; // optional namespace for use in console in case of duplicate names
				static Config mainConfig = null;

				static public bool isInited { get => consoleObject != null; }
				
				static readonly Dictionary<string, CfgField> cfgFields = new Dictionary<string, CfgField>();

				public class CfgField: Field
				{
					readonly BoundsAttribute bounds = null;

					public CfgField(object config, FieldInfo field): base(config, field)
					{
#if !DEBUG
						bounds = GetCustomAttribute(field, typeof(BoundsAttribute)) as BoundsAttribute;
#endif
					}

					override protected void setFieldValue(object value)
					{
						base.setFieldValue(bounds != null? bounds.applyBounds(value): value);
					}
				}


				static public void init(Config config, string _cfgNamespace = null)
				{
					if (consoleObject == null)
					{
						if (config != null)
						{
							consoleObject = PersistentConsoleCommands.createGameObject<SetGetCfgVarCommand>("ConfigConsoleCommands_" + Strings.modName);

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


				static CfgField getConfigField(string fieldName)
				{
					if (fieldName == null)
						return null;

					if (cfgNamespace != null)
					{
						if (fieldName.StartsWith(cfgNamespace))
							fieldName = fieldName.Replace(cfgNamespace + ".", "");
						else
							return null;
					}

					cfgFields.TryGetValue(fieldName, out CfgField cf);

					return cf;
				}


				static void setFieldValue(string fieldName, string fieldValue)
				{
					CfgField cf = getConfigField(fieldName);

					if (cf != null)
					{
						cf.value = fieldValue;
						mainConfig.save();
					}
				}


				static object getFieldValue(string fieldName)
				{
					return getConfigField(fieldName)?.value;
				}


				class SetGetCfgVarCommand: PersistentConsoleCommands
				{
					void OnConsoleCommand_setcfgvar(NotificationCenter.Notification n)
					{
						if (n?.data != null && n.data.Count == 2)
						{																			$"setcfgvar raw: '{n.data[0]}' '{n.data[1]}'".logDbg();
							setFieldValue(n.data[0] as string, n.data[1] as string);
						}
					}
					
					void OnConsoleCommand_getcfgvar(NotificationCenter.Notification n)
					{
						if (n?.data != null && n.data.Count == 1)
						{
							string fieldName = n.data[0] as string;
							object value = getFieldValue(fieldName);

							if (value != null)
								$"{fieldName} = {value}".onScreen();
						}
					}
				}
			}
			#endregion
		}
	}
}