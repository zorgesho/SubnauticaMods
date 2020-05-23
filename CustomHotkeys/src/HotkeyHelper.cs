using System.Linq;
using System.Collections;

using UnityEngine;
using SMLHelper.V2.Utility;

using Common;

namespace CustomHotkeys
{
	static class HotkeyHelper
	{
		static class CommandRunner
		{
			const char commandSeparator = ';';
			class MultipleCommands: MonoBehaviour {} // for coroutines

			static MultipleCommands multCmdRunner;

			public static void init()
			{
				multCmdRunner = hotkeyHelperGO.ensureComponent<MultipleCommands>();
			}

			static IEnumerator runMultiple(string command)
			{
				foreach (var cmd in command.Split(commandSeparator))
				{
					runCommand(cmd);
					yield return null;
				}
			}

			public static void runCommand(string command)
			{
				if (Main.config.switches.Find(sw => sw.id == command) is ModConfig.Switch sw)
					command = sw.commands[sw.index++ % sw.commands.Length];

				if (command.Contains(commandSeparator))
					multCmdRunner.StartCoroutine(runMultiple(command));
				else
					DevConsole.SendConsoleCommand(command);
			}
		}

		class HotkeyListener: MonoBehaviour
		{
			void Update()
			{
				foreach (var hotkey in Main.config.hotkeys)
				{
					if (KeyCodeUtils.GetKeyDown(hotkey.key.key) && (hotkey.key.modifier == KeyCode.None || KeyCodeUtils.GetKeyHeld(hotkey.key.modifier)))
						CommandRunner.runCommand(hotkey.command);
				}
			}
		}

		static GameObject hotkeyHelperGO;

		public static void init()
		{
			if (hotkeyHelperGO)
				return;

			hotkeyHelperGO = UnityHelper.createPersistentGameObject<HotkeyListener>("HotkeyHelper");
			CommandRunner.init();
		}
	}
}