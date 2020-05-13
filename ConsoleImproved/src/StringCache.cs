using System;
using System.Collections.Generic;

using Common;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		// used for console autocomplete
		abstract class StringCache
		{
			protected readonly List<string> strings = new List<string>();

			protected abstract void refresh();

			List<string> find(Predicate<string> predicate)
			{
				refresh();
				return strings.FindAll(predicate);
			}

			public List<string> find(string str)			=> find(s => s.IndexOf(str) >= 0);
			public List<string> findByPrefix(string prefix) => find(s => s.StartsWith(prefix));
		}

		// console commands
		class CommandCache: StringCache
		{
			protected override void refresh()
			{
				if (strings.Count == DevConsole.commands.Count)
					return;

				strings.Clear();
				DevConsole.commands.forEach(cmd => strings.Add(cmd.Key.ToLower() + " ")); // adding space for convenience

				strings.Sort();																		"ConsoleHelper.CommandCache refreshed".logDbg();
			}
		}

		// tech types as strings
		class TechTypeCache: StringCache
		{
			protected override void refresh()
			{
				if (strings.Count > 0) // techtypes don't change in runtime, so we need refresh this only once
					return;

				foreach (TechType t in Enum.GetValues(typeof(TechType)))
					strings.Add(t.AsString().ToLower());

				strings.Sort();
			}
		}

		// config fields exported to console
		class CfgVarsCache: StringCache
		{
			protected override void refresh()
			{
				if (strings.Count > 0)
					return;

				CfgVarsHelper.getVarNames(out List<string> fields);										fields.logDbg("CfgVarsCache added field ");

				for (int i = 0; i < fields.Count; i++)
					fields[i] += " "; // adding space for convenience

				strings.AddRange(fields);
			}
		}
	}
}