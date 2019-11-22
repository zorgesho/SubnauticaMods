using System;
using System.Collections;
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
				if (n.getArgsCount() == 0)
					return;

				List<string> matched = techtypeCache.find(n.getArg(0) as string);

				$"Finded {matched.Count} entries".onScreen();
				matched.onScreen("TechType: ");
			}


			void OnConsoleCommand_prefabdump(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() == 0)
					return;

				if (n.getArg(0) as string == "all")
				{
					StartCoroutine(nameof(_dumpAllPrefabs));
				}
				else
				{
					if (UWE.Utils.TryParseEnum(n.getArg(0) as string, out TechType techType))
						CraftData.GetPrefabForTechType(techType)?.dump(techType.AsString());
				}
			}

			IEnumerator _dumpAllPrefabs()
			{
				foreach (TechType techType in Enum.GetValues(typeof(TechType)))
				{
					CraftData.GetPrefabForTechType(techType)?.dump(techType.AsString());
					yield return null;
				}

				"Dump complete".onScreen();
			}

			void OnConsoleCommand_printcfgvars(NotificationCenter.Notification n)
			{
				string prefix = (n.getArgsCount() == 1)? n.getArg(0) as string: "";

				foreach (var c in cfgVarsCache.findByPrefix(prefix))
					DevConsole.SendConsoleCommand($"getcfgvar {c}");
			}
		}
	}
}