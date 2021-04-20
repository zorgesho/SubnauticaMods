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
	using Debug = Common.Debug;

	static partial class ConsoleHelper
	{
		static readonly string historyPath = Paths.modRootPath + "history.txt";

		static readonly CommandCache commandCache = new();
		static readonly CfgVarsCache cfgVarsCache = new();
		static readonly TechTypeCache techtypeCache = new();

		static void init()
		{
			PersistentConsoleCommands.register<ConsoleCommands>();
			DevConsole.disableConsole = !Main.config.consoleEnabled;
		}

		static void showMessages(List<string> msgs, string msg)
		{
#if GAME_SN
			int maxCount = ErrorMessageSettings.getSlotCount(true);
#else
			int maxCount = 30; // HACK
#endif
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

				StringBuilder sb = new();
				while (i < history.Count)
					sb.AppendLine(history[i++]);

				sb.ToString().saveToFile(historyPath);
			}
			catch (UnauthorizedAccessException e) { Log.msg(e); }
		}

		static void loadHistory()
		{
			if (!File.Exists(historyPath))
				return;

			string loadedHistory = File.ReadAllText(historyPath);

			if (!loadedHistory.isNullOrEmpty())
				setHistory(new List<string>(loadedHistory.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)));
		}

		static void setHistory(List<string> history)
		{
			DevConsole.instance.history = history;
			DevConsole.instance.inputField.SetHistory(history);
		}


		static class CfgVarsHelper
		{
			const string name_CfgVarBinder = "Common.Configuration.Utils." + nameof(Common.Configuration.Utils.CfgVarBinder);

			static List<string> cfgVarNames;
			static List<MethodWrapper<Func<string, object>>> cfgVarGetters;

			// search current assemblies for config vars binded to console
			static void init()
			{
				if (cfgVarNames != null)
					return;

				cfgVarNames = new List<string>();
				cfgVarGetters = new List<MethodWrapper<Func<string, object>>>();

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly.GetType(name_CfgVarBinder, false) is not Type type_CfgVarBinder)
						continue;

					var names = type_CfgVarBinder.method("getVarNames").wrap().invoke<string[]>();
					Debug.assert(names != null);
					cfgVarNames.AddRange(names);

					var getter = type_CfgVarBinder.method("getVarValue").wrap<Func<string, object>>();
					Debug.assert(getter);
					cfgVarGetters.Add(getter);
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