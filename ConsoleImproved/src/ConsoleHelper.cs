using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Reflection;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		static GameObject consoleObject = null;
		static readonly string historyPath = Paths.modRootPath + "history.txt";

		static readonly CommandCache commandCache = new CommandCache();
		static readonly CfgVarsCache cfgVarsCache = new CfgVarsCache();
		static readonly TechTypeCache techtypeCache = new TechTypeCache();

		static void init()
		{
			if (consoleObject == null)
			{
				consoleObject = PersistentConsoleCommands.createGameObject<ConsoleCommands>();
				DevConsole.disableConsole = !Main.config.consoleEnabled;
			}
		}

		static void showMessages(List<string> msgs, string msg)
		{
			int maxCount = ErrorMessageSettings.getSlotCount(true);

			if (Main.config.maxListSize > 0)
				maxCount = Math.Min(maxCount, Main.config.maxListSize);

			if (msgs.Count > maxCount)
				L10n.str("ids_listTooLarge").format(msgs.Count, maxCount - 1).onScreen();

			msgs.onScreen(msg, maxCount - 1);
		}

		static void saveHistory()
		{
			try
			{
				var history = DevConsole.instance.history;

				File.Delete(historyPath);

				// save 'historySizeToSave' last history entries or all history if historySizeToSave == 0
				int i = Main.config.historySizeToSave == 0? 0: Mathf.Max(0, history.Count - Main.config.historySizeToSave);

				StringBuilder sb = new StringBuilder();
				while (i < history.Count)
					sb.AppendLine(history[i++]);

				File.WriteAllText(historyPath, sb.ToString());
			}
			catch (UnauthorizedAccessException e) { Log.msg(e); }
		}

		static void loadHistory()
		{
			if (!File.Exists(historyPath))
				return;

			string loadedHistory = File.ReadAllText(historyPath);

			if (!loadedHistory.isNullOrEmpty())
				setHistory(new List<string>(loadedHistory.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)));
		}

		static void setHistory(List<string> history)
		{
			DevConsole.instance.history = history;
			DevConsole.instance.inputField.SetHistory(history);
		}


		static class CfgVarsHelper
		{
			const string exportCfgVarClassName = nameof(Common) + "." + nameof(Common.Configuration) + "." + nameof(Common.Configuration.ExportedCfgVarFields);
			const string exportCfgVarGetFields = nameof(Common.Configuration.ExportedCfgVarFields.getFields);
			const string getCfgVarValue = nameof(Common.Configuration.ExportedCfgVarFields.getFieldValue);

			static List<string> cfgVarNames;
			static List<MethodWrapper<Func<string, object>>> cfgVarGetters;

			// searching exported config fields in current assemblies
			static void init()
			{
				if (cfgVarNames != null)
					return;

				cfgVarNames = new List<string>();
				cfgVarGetters = new List<MethodWrapper<Func<string, object>>>();

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.GetType(exportCfgVarClassName, false) is Type exportedCfgVars)
					{
						if (exportedCfgVars.method(exportCfgVarGetFields).wrap().invoke() is IList<string> list)
							cfgVarNames.AddRange(list);

						var getter = exportedCfgVars.method(getCfgVarValue).wrap<Func<string, object>>();
						if (getter)
							cfgVarGetters.Add(getter);
					}
				}
			}

			public static void getVarNames(out List<string> fields)
			{
				init();
				fields = new List<string>(cfgVarNames);
			}

			public static object getVarValue(string cfgVarName)
			{
				init();
				return cfgVarGetters.Select(getter => getter.invoke(cfgVarName)).FirstOrDefault(result => result != null);
			}
		}
	}
}