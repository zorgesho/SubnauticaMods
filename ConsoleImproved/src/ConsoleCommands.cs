using System.Collections.Generic;

using Common;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		class ConsoleCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_clearhistory(NotificationCenter.Notification n)
			{
				setHistory(new List<string>());
			}

			void OnConsoleCommand_clear(NotificationCenter.Notification n)
			{
				foreach (var m in ErrorMessage.main.messages)
					m.timeToDelete = 0f;
			}

			void OnConsoleCommand_findtech(NotificationCenter.Notification n)
			{
				if (n?.data == null || n.data.Count == 0)
					return;

				List<string> matched = techtypeCache.find(n.data[0] as string);

				$"Finded {matched.Count} entries".onScreen();
				matched.onScreen("TechType: ");
			}

			// todo: remove
			void OnConsoleCommand_test1(NotificationCenter.Notification n)
			{
				"TEST1".onScreen();
			}
			void OnConsoleCommand_test2(NotificationCenter.Notification n)
			{
				"TEST2".onScreen();
			}
		}
	}
}