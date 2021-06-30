using System;
using System.Linq;
using System.Collections.Generic;

using Common;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		// used for console autocomplete
		abstract class StringCache
		{
			protected List<string> strings;

			protected abstract void refresh();

			List<string> find(Predicate<string> predicate)
			{
				refresh();
				return strings?.FindAll(predicate);
			}

			public List<string> find(string str)			=> find(s => s.Contains(str));
			public List<string> findByPrefix(string prefix) => find(s => s.startsWith(prefix));
		}

		// console commands
		class CommandCache: StringCache
		{
			protected override void refresh()
			{
				if (strings?.Count == DevConsole.commands.Count)
					return;

				strings = DevConsole.commands.Select(cmd => cmd.Key.ToLower() + " ").ToList(); // adding space for convenience
				strings.Sort();
			}
		}

		// tech types as strings
		class TechTypeCache: StringCache
		{
			protected override void refresh()
			{
				if (strings?.Count > 0) // techtypes don't change in runtime, so we need refresh this only once
					return;

				strings = Enum.GetValues(typeof(TechType)).OfType<TechType>().Select(t => t.AsString().ToLower()).ToList();
				strings.Sort();
			}
		}

		// config fields exported to console
		class CfgVarsCache: StringCache
		{
			protected override void refresh()
			{
				if (strings?.Count > 0)
					return;

				CfgVarsHelper.getVarNames(out List<string> fields);									$"CfgVarsCache fields added:\r\n{string.Join("\r\n", fields)}".logDbg();

				strings = fields.Select(field => field + " ").ToList(); // adding space for convenience
				strings.Sort();
			}
		}
	}
}