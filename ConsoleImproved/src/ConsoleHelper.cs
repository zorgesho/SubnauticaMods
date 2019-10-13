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
		static readonly string historyPath = Common.PathHelper.Paths.modRootPath + "history.txt";

		class ConsoleCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_clearhistory(NotificationCenter.Notification n)
			{
				setHistory(new List<string>());
			}
		}


		static void init()
		{
			if (consoleObject == null)
			{
				consoleObject = PersistentConsoleCommands.createGameObject<ConsoleCommands>();
				DevConsole.disableConsole = !Main.config.consoleEnabled;
			}
		}


		static void saveHistory()
		{
			if (File.Exists(historyPath)) // TODO: check permissions, maybe trycatch
				File.Delete(historyPath);
			
			List<string> history = DevConsole.instance.history;

			// save 'historySizeToSave' last history entries or all history if historySizeToSave == 0
			int i = Main.config.historySizeToSave == 0? 0: Mathf.Max(0, history.Count - Main.config.historySizeToSave);

			StringBuilder stringBuilder = new StringBuilder();
			while (i < history.Count)
				stringBuilder.AppendLine(history[i++]);

			File.WriteAllText(historyPath, stringBuilder.ToString());
		}
		
		
		static void loadHistory()
		{
			if (!File.Exists(historyPath))
				return;

			string loadedHistory = File.ReadAllText(historyPath);

			if (!string.IsNullOrEmpty(loadedHistory))
			{
				string[] lines = loadedHistory.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

				setHistory(new List<string>(lines));
			}
		}


		static void setHistory(List<string> history)
		{
			DevConsole.instance.history = history;
			DevConsole.instance.inputField.SetHistory(history);
		}
	}
}