using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using UnityEngine;

using Common;

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
			int maxCount = Main.config.msgsSettings.currMaxListSize;

			if (msgs.Count > maxCount)
				L10n.str("ids_listTooLarge").format(msgs.Count, maxCount).onScreen();

			msgs.onScreen(msg, maxCount);
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
	}
}