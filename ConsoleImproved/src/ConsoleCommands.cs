using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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
					m.timeEnd = Time.time - 1;
			}


			void OnConsoleCommand_findtech(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() == 0)
					return;

				List<string> matched = techtypeCache.find(n.getArg(0) as string);

				$"Finded {matched.Count} entries".onScreen();
				matched.onScreen("TechType: ");
			}

			void OnConsoleCommand_dumpresource(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() == 0)
					return;

				Resources.Load<GameObject>(n.getArg(0) as string)?.dump();
			}

			void OnConsoleCommand_dumptarget(NotificationCenter.Notification _)
			{
				if (Player.main?.GetComponent<GUIHand>().activeTarget is GameObject go)
				{
					$"Target dump: {go.name}".onScreen();
					go.dump();
				}
			}

			void OnConsoleCommand_dumpprefab(NotificationCenter.Notification n)
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


			void OnConsoleCommand_dumpobjects(NotificationCenter.Notification n)
			{
				Type getComponentType(string typeName)
				{
					foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
					{
						Type type = assembly.GetType(typeName, false, true);

						if (type != null && typeof(Component).IsAssignableFrom(type))
							return type;
					}

					return null;
				}

				if (n.getArgsCount() == 0)
					return;

				string cmtTypeName = n.getArg(0) as string;
				Type cmpType = getComponentType(cmtTypeName);

				if (cmpType != null && FindObjectsOfType(cmpType) is Component[] cmps)
					StartCoroutine(nameof(_dumpObjects), cmps);
			}

			IEnumerator _dumpObjects(Component[] cmps)
			{
				$"Objects to dump: {cmps.Length}".onScreen();

				int index = 0;
				foreach (var cmp in cmps)
				{
					cmp.gameObject.dump(cmp.gameObject.name + "_" + index++);
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