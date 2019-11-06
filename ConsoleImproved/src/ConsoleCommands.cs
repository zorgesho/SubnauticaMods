using System.Collections.Generic;
using Common;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		class ConsoleCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_clearhistory(NotificationCenter.Notification _)
			{
				setHistory(new List<string>());
			}


			void OnConsoleCommand_clear(NotificationCenter.Notification _)
			{
				foreach (var m in ErrorMessage.main.messages)
					m.timeEnd = UnityEngine.Time.time - 1;
			}


			void OnConsoleCommand_findtech(NotificationCenter.Notification n)
			{
				if (n?.data == null || n.data.Count == 0)
					return;

				List<string> matched = techtypeCache.find(n.data[0] as string);

				$"Finded {matched.Count} entries".onScreen();
				matched.onScreen("TechType: ");
			}


			void OnConsoleCommand_printcfgvars(NotificationCenter.Notification n)
			{
				string prefix = (n?.data != null && n.data.Count == 1)? n.data[0] as string: "";

				foreach (var c in cfgVarsCache.findByPrefix(prefix))
					DevConsole.SendConsoleCommand($"getcfgvar {c}");
			}
		}
	}
}