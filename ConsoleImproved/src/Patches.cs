using System;
using System.IO;
using System.Text;
using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using Harmony;

using Common;

namespace ConsoleImproved
{
	// patch for full history in console
	[HarmonyPatch(typeof(ConsoleInput), "Validate")]
	static class ConsoleInput_Validate_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool injected = false;

			foreach (var instruction in instructions)
			{
				if (!injected && instruction.opcode.Equals(OpCodes.Ldfld))
				{
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ConsoleInput), "historyIndex"));
					continue;
				}

				if (!injected && instruction.opcode.Equals(OpCodes.Callvirt))
				{
					injected = true;
					yield return new CodeInstruction(OpCodes.Nop);
					continue;
				}

				yield return instruction;
			}
		}
	}

	[HarmonyPatch(typeof(DevConsole), "Awake")]
	static class DevConsole_Awake_Patch
	{
		static void Postfix(DevConsole __instance)
		{
			ConsoleHelper.init();
			ConsoleHelper.loadHistory();
		}
	}

	[HarmonyPatch(typeof(DevConsole), "OnDisable")]
	static class DevConsole_OnDisable_Patch
	{
		static void Postfix(DevConsole __instance)
		{
			ConsoleHelper.saveHistory();
		}
	}

	//private bool KeyPressedOverride(Event evt)
	//[HarmonyPatch(typeof(ConsoleInput), "KeyPressedOverride")]
	//static class DevConsole_Awake_Patch11
	//{
	//	static bool Prefix(ConsoleInput __instance, Event evt, ref bool __result)
	//	{
	//		KeyCode keyCode = __instance.processingEvent.keyCode;

	//		if (keyCode == KeyCode.Tab)
	//		{
	//			"TAB".onScreen();
	//			__instance.text += "ololo";//.onScreen();
	//		}

	//		return true;
	//	}
	//}


	static class ConsoleHelper
	{
		static GameObject consoleObject = null;
		static readonly string historyPath = Common.PathHelper.Paths.modRootPath + "history.txt";

		static public void init()
		{
			if (consoleObject == null)
			{
				consoleObject = PersistentConsoleCommands.createGameObject<ConsoleCommands>();
				DevConsole.disableConsole = !Main.config.consoleEnabled;
			}
		}

		static public void saveHistory()
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
		
		
		static public void loadHistory()
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

		static public void setHistory(List<string> history)
		{
			DevConsole.instance.history = history;
			DevConsole.instance.inputField.SetHistory(history);
		}

		
		class ConsoleCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_clearhistory(NotificationCenter.Notification n)
			{
				setHistory(new List<string>());
			}
		}
	}
}