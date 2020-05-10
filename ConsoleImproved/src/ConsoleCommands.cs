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
			void OnConsoleCommand_logpatches(NotificationCenter.Notification n)
			{
				$"Current patches:\n{HarmonyHelper.getPatchesReport(n.getArg(0))}".log();
			}

			void OnConsoleCommand_printpatches(NotificationCenter.Notification n)
			{
				const float refreshSecs = 0.5f;

				StopAllCoroutines();

				if (n.getArgCount() > 0)
					StartCoroutine(_printPatches(n.getArg(0), n.getArg<bool>(1)));

				static IEnumerator _printPatches(string harmonyID, bool omitNames)
				{
					while (true)
					{
						$"\n{HarmonyHelper.getPatchesReport(harmonyID, omitNames)}".onScreen($"patches ({harmonyID})");
						yield return new WaitForSeconds(refreshSecs);
					}
				}
			}


			void OnConsoleCommand_clearhistory(NotificationCenter.Notification _)
			{
				setHistory(new List<string>());
			}

			void OnConsoleCommand_clear(NotificationCenter.Notification _)
			{
				GameUtils.clearScreenMessages();
			}


			void OnConsoleCommand_findtech(NotificationCenter.Notification n)
			{
				if (n.getArgCount() == 0)
					return;

				var matched = techtypeCache.find(n.getArg(0));

				$"Finded {matched.Count} entries".onScreen();
				matched.onScreen("TechType: ");
			}


			void OnConsoleCommand_dumpresource(NotificationCenter.Notification n)
			{
				if (n.getArgCount() == 0)
					return;

				Resources.Load<GameObject>(n.getArg(0))?.dump();
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
				if (n.getArgCount() == 0)
					return;

				if (n.getArg(0) == "all")
				{
					StartCoroutine(_dumpAllPrefabs());
				}
				else
				{
					if (UWE.Utils.TryParseEnum(n.getArg(0), out TechType techType))
						CraftData.GetPrefabForTechType(techType)?.dump(techType.AsString());
				}

				static IEnumerator _dumpAllPrefabs()
				{
					foreach (TechType techType in Enum.GetValues(typeof(TechType)))
					{
						CraftData.GetPrefabForTechType(techType)?.dump(techType.AsString());
						yield return null;
					}

					"Dump complete".onScreen();
				}
			}

			void OnConsoleCommand_dumpobjects(NotificationCenter.Notification n)
			{
				if (n.getArgCount() == 0)
					return;

				static Type getComponentType(string typeName)
				{
					foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
					{
						Type type = assembly.GetType(typeName, false, true);

						if (type != null && typeof(Component).IsAssignableFrom(type))
							return type;
					}

					return null;
				}

				Type cmpType = getComponentType(n.getArg(0));

				if (cmpType != null && FindObjectsOfType(cmpType) is Component[] cmps)
				{
					StartCoroutine(_dumpObjects());

					IEnumerator _dumpObjects()
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
				}
			}


			void OnConsoleCommand_printcfgvars(NotificationCenter.Notification n)
			{
				foreach (var c in cfgVarsCache.findByPrefix(n.getArg(0) ?? ""))
					DevConsole.SendConsoleCommand($"getcfgvar {c}");
			}
		}
	}
}