using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Config
{
	partial class Config
	{
		[AttributeUsage(AttributeTargets.Class)]
		public class AddFieldsToConsoleAttribute: ConfigAttribute
		{
			readonly string cfgNamespace = null;

			public AddFieldsToConsoleAttribute(string _cfgNamespace = null) => cfgNamespace = _cfgNamespace;
			
			override public void process(object config)
			{
				if (config is Config)
					ConfigVarsConsoleCommand.init(config as Config, cfgNamespace);

				FieldInfo[] fields = config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				foreach (FieldInfo field in fields)
				{
					if (field.FieldType.IsPrimitive)
						ConfigVarsConsoleCommand.addCfgField(config, field);
					
					if (field.FieldType.IsClass)
						process(field.GetValue(config));
				}
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

			struct ConfigField
			{
				public object config;
				public FieldInfo field;
			}

			static readonly Dictionary<string, ConfigField> cfgFields = new Dictionary<string, ConfigField>();

			static public void init(Config config, string _cfgNamespace = null)
			{
				if (consoleObject == null)
				{
					if (config != null)
					{
						consoleObject = new GameObject("ConfigConsoleCommands", typeof(SetCfgVarCommand), typeof(SceneCleanerPreserve));
						mainConfig = config;
						cfgNamespace = _cfgNamespace;
					}
					else
						"ConfigVarsConsoleCommand.init mainConfig is null !".logError();
				}
			}
			
			static public void addCfgField(object config, FieldInfo field)
			{																									$"ConfigVarsConsoleCommand: adding field {field.Name}".logDbg();
				string name = field.Name.ToLower();

				if (cfgFields.ContainsKey(name))
					$"ConfigVarsConsoleCommand: field {name} is already added!".logError();
				else
					cfgFields[name] = new ConfigField { config = config, field = field };
			}


			static void registerCommand(SetCfgVarCommand cmp)
			{
				UnityEngine.Object.DontDestroyOnLoad(cmp.gameObject);
				DevConsole.RegisterConsoleCommand(cmp, cmdName);
#if DEBUG
				DevConsole.disableConsole = false;
#endif
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
						cf.field.SetValue(cf.config, Convert.ChangeType(fieldValue, cf.field.FieldType));
						mainConfig.save();
					}
				}
				catch (Exception e)
				{
					Log.msg(e);
				}
			}

			class SetCfgVarCommand: MonoBehaviour
			{
				void Awake()
				{
					registerCommand(this);
					SceneManager.sceneUnloaded += onSceneUnloaded;
				}

				void OnDestroy() => SceneManager.sceneUnloaded -= onSceneUnloaded;

				void onSceneUnloaded(Scene scene)
				{
					// notifications are cleared between some scenes, so we need to reregister command
					if (NotificationCenter.DefaultCenter.notifications["OnConsoleCommand_" + cmdName] == null)
						registerCommand(this);
				}

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