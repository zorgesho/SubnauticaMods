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
			protected List<string> strings = new List<string>();

			abstract protected void refresh();

			List<string> find(Predicate<string> predicate)
			{
				refresh();
				return strings.FindAll(predicate);
			}

			public List<string> findByPrefix(string prefix)
			{
				return find((s) => s.StartsWith(prefix));
			}

			public List<string> find(string str)
			{
				return find((s) => s.IndexOf(str) >= 0);
			}
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
						strings.Add(cmd.Key.ToLower());

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
	}
}