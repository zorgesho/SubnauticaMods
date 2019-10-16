using System;
using System.Reflection;
using System.Collections.Generic;

using Common;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		// used for console autocomplete
		abstract class StringCache
		{
			protected List<string> strings = new List<string>();

			abstract protected void refresh();

			List<string> find(Predicate<string> predicate)
			{
				refresh();
				return strings.FindAll(predicate);
			}

			public List<string> find(string str)			=> find((s) => s.IndexOf(str) >= 0);
			public List<string> findByPrefix(string prefix) => find((s) => s.StartsWith(prefix));
		}

		// console commands
		class CommandCache: StringCache
		{
			override protected void refresh()
			{
				if (strings.Count != DevConsole.commands.Count)
				{
					strings.Clear();

					foreach (var cmd in DevConsole.commands)
						strings.Add(cmd.Key.ToLower() + " "); // add space for convenience

					strings.Sort();										"ConsoleHelper.CommandCache refreshed".logDbg();
				}
			}
		}

		// tech types as strings
		class TechTypeCache: StringCache
		{
			override protected void refresh()
			{
				if (strings.Count == 0) // techtypes don't change in runtime, so we need refresh this only once
				{
					foreach (TechType t in Enum.GetValues(typeof(TechType)))
						strings.Add(t.AsString().ToLower());

					strings.Sort();
				}
			}
		}

		// config fields exported to console
		class CfgVarsCache: StringCache
		{
			static readonly string exportCfgVarClassName = nameof(Common) + "." + nameof(Common.Config) + "." + nameof(Common.Config.ExportedCfgVarFields);

			// searching exported config fields in current assemblies
			override protected void refresh()
			{
				if (strings.Count == 0)
				{
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

					foreach (var a in assemblies)
					{
						Type cfgvars = a.GetType(exportCfgVarClassName, false);

						if (cfgvars != null)
						{
							List<string> fields = cfgvars.GetMethod("getFields")?.Invoke(null, null) as List<string>;
							if (fields != null)
							{
								strings.AddRange(fields);
								fields.logDbg("CfgVarsCache added field ");
							}
						}
					}
				}
			}
		}
	}
}