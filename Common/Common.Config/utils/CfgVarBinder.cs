using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration.Utils
{
	static class CfgVarBinder
	{
		class CfgVarCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_setcfgvar(NotificationCenter.Notification n)
			{
				if (n.getArgCount() == 2)
				{																			$"setcfgvar: '{n.getArg(0)}' '{n.getArg(1)}'".logDbg();
					setVarValue(n.getArg(0), n.getArg(1));
				}
			}

			void OnConsoleCommand_getcfgvar(NotificationCenter.Notification n)
			{																				$"getcfgvar: '{n.getArg(0)}'".logDbg();
				if (n.getArgCount() != 1)
					return;

				string fieldName = n.getArg(0);

				if (getVarValue(fieldName) is object value)
					$"{fieldName} = {value}".onScreen();
			}
		}
#pragma warning disable IDE0052
		static GameObject consoleCommands;
#pragma warning restore

		static void init() => consoleCommands ??= PersistentConsoleCommands.createGameObject<CfgVarCommands>("CfgVarCommands_" + Mod.id);

		static readonly UniqueIDs uniqueIDs = new UniqueIDs();
		static readonly Dictionary<string, Config.Field> cfgFields = new Dictionary<string, Config.Field>();

		public static string[] getVarNames() => cfgFields.Keys.ToArray();

		public static void addField(Config.Field cfgField, string varNamespace = null)
		{																									$"CfgVarBinder: adding field {cfgField.id}".logDbg();
			init();

			string varName = ((varNamespace.isNullOrEmpty()? "": varNamespace + ".") + cfgField.id).ToLower();
			uniqueIDs.ensureUniqueID(ref varName);

			cfgFields[varName] = cfgField;
		}

		static Config.Field getField(string name)
		{
			return name != null && cfgFields.TryGetValue(name, out Config.Field cf)? cf: null;
		}

		static void setVarValue(string name, string value)
		{
			if (getField(name) is Config.Field cf)
				cf.value = value;
		}

		static object getVarValue(string name)
		{
			return getField(name)?.value;
		}
	}
}