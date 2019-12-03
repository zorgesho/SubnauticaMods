using UnityEngine;
using Common;

namespace MiscPatches
{
	static class ConsoleCommands
	{
		static GameObject go = null;

		class Commands: PersistentConsoleCommands
		{
			void OnConsoleCommand_printinventory(NotificationCenter.Notification _)
			{
				if (!Inventory.main)
					return;

				"Inventory items:".log();
				foreach (var item in Inventory.main.container)
					$"item: {item.item.GetTechName()}".onScreen().log();
			}

			void OnConsoleCommand_subtitles(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() > 0)
					Subtitles.main.Add(n.getArg(0) as string);
			}
		}

		public static void init()
		{
			if (!go)
				go = PersistentConsoleCommands.createGameObject<Commands>("MiscPatches.ConsoleCommands");
		}
	}
}