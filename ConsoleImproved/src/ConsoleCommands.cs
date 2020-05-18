using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Harmony;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		class ConsoleCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_logpatches(NotificationCenter.Notification n)
			{
				$"Current patches:\n{PatchesReport.get(n.getArg(0))}".log();
			}

			void OnConsoleCommand_pinpatches(NotificationCenter.Notification n)
			{
				const float refreshSecs = 0.5f;

				StopAllCoroutines();
				GameUtils.clearScreenMessages();

				if (n.getArgCount() > 0)
					StartCoroutine(_printPatches(n.getArg(0), n.getArg<bool>(1)));

				static IEnumerator _printPatches(string harmonyID, bool omitNames)
				{
					while (true)
					{
						$"\n{PatchesReport.get(harmonyID, omitNames)}".onScreen($"patches ({harmonyID})");
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

				L10n.str("ids_findEntries").format(matched.Count).onScreen();
				showMessages(matched, L10n.str("ids_techType"));
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

			void OnConsoleCommand_pincfgvars(NotificationCenter.Notification n)
			{
				const float refreshSecs = 0.1f;
				const float varColorChangeTime = 2f;

				StopAllCoroutines();
				GameUtils.clearScreenMessages();

				if (n.getArgCount() > 0)
					StartCoroutine(_printCfgVars(n.getArg(0) == "all"? "": n.getArg(0)));

				static IEnumerator _printCfgVars(string prefix)
				{
					var varNames = cfgVarsCache.findByPrefix(prefix).Select(name => name.Trim()).ToList();
					var prevValues = new Dictionary<string, Tuple<object, float>>(); // key: var name, value: item1 - var value, item2 - last change time
					var sb = new StringBuilder();

					while (true)
					{
						sb.Clear();
						sb.AppendLine();

						foreach (var varName in varNames)
						{
							bool changeColor = false;
							var varValue = CfgVarsHelper.getVarValue(varName);

							if (prevValues.TryGetValue(varName, out var prevValue))
							{
								if (!Equals(prevValue.Item1, varValue))
								{
									prevValues[varName] = Tuple.Create(varValue, Time.realtimeSinceStartup);
									changeColor = true;
								}
								else
								{
									changeColor = Time.realtimeSinceStartup - prevValue.Item2 < varColorChangeTime;
								}
							}
							else
							{
								prevValues[varName] = Tuple.Create(varValue, 0f);
							}

							sb.AppendLine($"{(changeColor?"<color=magenta>":"")}{varName} = {varValue ?? "<color=red>[null]</color>"}{(changeColor?"</color>":"")}");
						}

						sb.ToString().onScreen($"cfg vars: {(prefix == ""? "all": prefix)}");

						yield return refreshSecs == 0f? null: new WaitForSeconds(refreshSecs);
					}
				}
			}
		}
	}
}