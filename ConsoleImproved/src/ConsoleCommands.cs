using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Harmony;
using Common.Crafting;
using Common.Reflection;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		class ConsoleCommands: PersistentConsoleCommands
		{
			public void logpatches(string harmonyID = null)
			{
				$"Current patches:\r\n{PatchesReport.get(harmonyID)}".log();
			}

			public void pinpatches(string harmonyID = null, bool omitNames = false)
			{
				const float refreshSecs = 1.0f;

				StopAllCoroutines();
				GameUtils.clearScreenMessages();

				if (harmonyID == null)
					return;

				StartCoroutine(_printPatches(harmonyID, omitNames));

				static IEnumerator _printPatches(string harmonyID, bool omitNames)
				{
					while (true)
					{
						$"\n{PatchesReport.get(harmonyID, omitNames)}".onScreen($"patches ({harmonyID})");
						yield return new WaitForSeconds(refreshSecs);
					}
				}
			}


			public void clearhistory()
			{
				setHistory(new List<string>());
			}

			public void clear()
			{
				GameUtils.clearScreenMessages();
			}


			public void findtech(string matchStr)
			{
				var matched = techtypeCache.find(matchStr);

				L10n.str("ids_findEntries").format(matched.Count).onScreen();
				showMessages(matched, L10n.str("ids_techType"));
			}


			public void dumpresource(string resourcePath)
			{
				Resources.Load<GameObject>(resourcePath)?.dump();
			}

			public void dumptarget()
			{
				if (Player.main?.GetComponent<GUIHand>().activeTarget is GameObject go)
				{
					$"Target dump: {go.name}".onScreen();
					go.dump();
				}
			}

			public void dumpprefab(string techTypeStr)
			{
				StartCoroutine(techTypeStr == "all"? _dumpAllPrefabs(): _dumpPrefab(techTypeStr.convert<TechType>()));

				static IEnumerator _dumpPrefab(TechType techType)
				{
					var task = PrefabUtils.getPrefabAsync(techType);
					yield return task;
					task.GetResult()?.dump(techType.AsString());
				}

				static IEnumerator _dumpAllPrefabs()
				{
					foreach (TechType techType in Enum.GetValues(typeof(TechType)))
						yield return _dumpPrefab(techType);

					"Dump complete".onScreen();
				}
			}

			public void dumpobjects(string componentType, int dumpParent = 0)
			{
				static Type _getComponentType(string typeName)
				{
					foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
						if (assembly.GetType(typeName, false, true) is Type type && typeof(Component).IsAssignableFrom(type))
							return type;

					return null;
				}

				Type cmpType = _getComponentType(componentType);

				if (cmpType != null && FindObjectsOfType(cmpType) is Component[] cmps)
				{
					StartCoroutine(_dumpObjects());

					IEnumerator _dumpObjects()
					{
						$"Objects to dump: {cmps.Length}".onScreen();

						int index = 0;
						foreach (var cmp in cmps)
						{
							cmp.gameObject.dump(cmp.gameObject.name + "_" + index++, dumpParent);
							yield return null;
						}

						"Dump complete".onScreen();
					}
				}
			}

			public void dumpscene()
			{
				FindObjectOfType<StoreInformationIdentifier>()?.transform.root.gameObject.dump("scene-dump");
			}


			public void togglecfgvar(string varName)
			{
				bool varValue = CfgVarsHelper.getVarValue(varName).convert<bool>();

				DevConsole.SendConsoleCommand($"setcfgvar {varName} {!varValue}");
				CfgVarsHelper.getVarValue(varName)?.ToString().onScreen(varName);
			}

			public void printcfgvars(string cfgvarPrefix = "")
			{
				foreach (var c in cfgVarsCache.findByPrefix(cfgvarPrefix))
					DevConsole.SendConsoleCommand($"getcfgvar {c}");
			}

			public void pincfgvars(string cfgvarPrefix = null) // null for stop
			{
				const float refreshSecs = 0.1f;
				const float varColorChangeTime = 2f;

				StopAllCoroutines();
				GameUtils.clearScreenMessages();

				if (cfgvarPrefix != null)
					StartCoroutine(_printCfgVars(cfgvarPrefix == "all"? "": cfgvarPrefix));

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