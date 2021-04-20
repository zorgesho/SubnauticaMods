using System.Linq;
using System.Collections.Generic;

namespace Common.Configuration.Utils
{
	static class CfgVarBinder
	{
		class CfgVarCommands: PersistentConsoleCommands
		{
			public void setcfgvar(string varName, string varValue)
			{																				$"setcfgvar: '{varName}' '{varValue}'".logDbg();
				setVarValue(varName, varValue);
			}

			public void getcfgvar(string varName)
			{																				$"getcfgvar: '{varName}'".logDbg();
				if (getVarValue(varName) is object value)
					$"{varName} = {value}".onScreen();
			}
		}

		static void init() => PersistentConsoleCommands.register<CfgVarCommands>();

		static readonly UniqueIDs uniqueIDs = new();
		static readonly Dictionary<string, Config.Field> cfgFields = new();

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