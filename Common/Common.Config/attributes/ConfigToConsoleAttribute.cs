using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

namespace Common.Configuration
{
	using Reflection;

	// for use in other mods
	public static class ExportedCfgVarFields
	{
		static readonly List<string> fieldNames = new List<string>();
		static readonly Dictionary<string, Config.Field> fields = new Dictionary<string, Config.Field>();

		public static ReadOnlyCollection<string> getFields() => fieldNames.AsReadOnly();

		public static object getFieldValue(string fieldName) => fields.TryGetValue(fieldName, out Config.Field field)? field.value: null;

		internal static void addField(string fieldName, Config.Field field)
		{
			fieldNames.Add(fieldName);
			fields[fieldName] = field;
		}
	}

	partial class Config
	{
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
		public class AddToConsoleAttribute: Attribute, IConfigAttribute, IFieldAttribute, IRootConfigInfo
		{
			[AttributeUsage(AttributeTargets.Field)]
			public class SkipAttribute: Attribute {} // don't add field to console

			static readonly UniqueIDs uniqueIDs = new UniqueIDs();

			readonly string cfgNamespace = ""; // optional namespace for use in console in case of duplicate names
			readonly bool addPrivateFields, ignoreSkipAttr;

			Config rootConfig;
			public void setRootConfig(Config config) => rootConfig = config;

			public AddToConsoleAttribute(string cfgNamespace = null, bool addPrivateFields = false, bool ignoreSkipAttr = false)
			{
				if (!cfgNamespace.isNullOrEmpty())
					this.cfgNamespace = cfgNamespace + ".";

				this.addPrivateFields = addPrivateFields;
				this.ignoreSkipAttr = ignoreSkipAttr;
			}

			public void process(object config)
			{
				config.GetType().fields().forEach(field => process(config, field));
			}

			public void process(object config, FieldInfo field)
			{
				ConfigVarsConsoleCommand.init();

				if (field.FieldType.IsPrimitive && (addPrivateFields || field.IsPublic) && (ignoreSkipAttr || !field.checkAttr<SkipAttribute>()))
				{
					var cfgField = new ConfigVarsConsoleCommand.CfgField(config, field, rootConfig);
					string nameForConsole = (cfgNamespace + cfgField.path).ToLower();
					uniqueIDs.ensureUniqueID(ref nameForConsole);

					if (ConfigVarsConsoleCommand.addConfigField(nameForConsole, cfgField))
						ExportedCfgVarFields.addField(nameForConsole, cfgField);
				}

				if (_isInnerFieldsProcessable(field))
					process(field.GetValue(config));
			}

#region Internal stuff
			static class ConfigVarsConsoleCommand
			{
#pragma warning disable IDE0052
				static GameObject consoleCommands = null;
#pragma warning restore
				static readonly Dictionary<string, CfgField> cfgFields = new Dictionary<string, CfgField>();

				public class CfgField: Field
				{
					readonly RangeAttribute range;

					public CfgField(object parent, FieldInfo field, Config rootConfig): base(parent, field, rootConfig)
					{
						range = getAttr<RangeAttribute>();
					}

					public override object value
					{
						set => base.value = range != null? range.clamp(value): value;
					}
				}


				public static void init()
				{
					consoleCommands ??= PersistentConsoleCommands.createGameObject<SetGetCfgVarCommand>("ConfigConsoleCommands_" + Mod.id);
				}

				public static bool addConfigField(string nameForConsole, CfgField cfgField)
				{																									$"ConfigVarsConsoleCommand: adding field {nameForConsole}".logDbg();
					if (cfgFields.ContainsKey(nameForConsole))
					{
						$"ConfigVarsConsoleCommand: field {nameForConsole} is already added!".logError();
						return false;
					}
					else
					{
						cfgFields[nameForConsole] = cfgField;
						return true;
					}
				}

				static CfgField getConfigField(string fieldName)
				{
					if (fieldName == null)
						return null;

					cfgFields.TryGetValue(fieldName, out CfgField cf);

					return cf;
				}

				static void setFieldValue(string fieldName, string fieldValue)
				{
					if (getConfigField(fieldName) is CfgField cf)
						cf.value = fieldValue;
				}

				static object getFieldValue(string fieldName)
				{
					return getConfigField(fieldName)?.value;
				}


				class SetGetCfgVarCommand: PersistentConsoleCommands
				{
					void OnConsoleCommand_setcfgvar(NotificationCenter.Notification n)
					{
						if (n.getArgCount() == 2)
						{																			$"setcfgvar: '{n.getArg(0)}' '{n.getArg(1)}'".logDbg();
							setFieldValue(n.getArg(0), n.getArg(1));
						}
					}

					void OnConsoleCommand_getcfgvar(NotificationCenter.Notification n)
					{																				$"getcfgvar: '{n.getArg(0)}'".logDbg();
						if (n.getArgCount() != 1)
							return;

						string fieldName = n.getArg(0);
						object value = getFieldValue(fieldName);

						if (value != null)
							$"{fieldName} = {value}".onScreen();
					}
				}
			}
#endregion
		}
	}
}