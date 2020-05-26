using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Utility;

using Common;

namespace CustomHotkeys
{
	static class HotkeyHelper
	{
		abstract class Hotkey
		{
			public InputHelper.KeyWithModifier key;

			protected Hotkey(InputHelper.KeyWithModifier key)
			{
				this.key = key;
			}

			public void update()
			{
				if (getKeyState())
					runCmd();
			}

			bool getKeyState() // TODO hold mode && check for unpressed modifiers
			{
				return KeyCodeUtils.GetKeyDown(key.key) && (key.modifier == KeyCode.None || KeyCodeUtils.GetKeyHeld(key.modifier));
			}

			protected abstract void runCmd();
		}

		class HotkeyCommand: Hotkey
		{
			readonly string[] commands;
			public HotkeyCommand(InputHelper.KeyWithModifier key, string[] commands): base(key) => this.commands = commands;

			protected override void runCmd() => CommandRunner.run(commands);
		}

		class HotkeySwitch: Hotkey
		{
			int index;
			readonly string[][] commands;
			public HotkeySwitch(InputHelper.KeyWithModifier key, string[][] commands): base(key) => this.commands = commands;

			protected override void runCmd() => CommandRunner.run(commands[index++ % commands.Length]);
		}


		static class HotkeyInitializer
		{
			const char switchSeparator = '|';
			const char commandSeparator = ';';

			static List<Tuple<Hotkey, ModConfig.Hotkey>> keysInfo;

			public static void updateBinds() => keysInfo.ForEach(info => info.Item1.key = info.Item2.key);

			public static List<Hotkey> create(List<ModConfig.Hotkey> keys)
			{
				keysInfo = keys.Select(hk => Tuple.Create(create(hk), hk)).ToList();
				return keysInfo.Select(hk => hk.Item1).ToList();
			}

			static Hotkey create(ModConfig.Hotkey hotkey)
			{
				Hotkey newHotkey;

				if (hotkey.command.Contains(switchSeparator))
				{
					string[]   switches = hotkey.command.Split(switchSeparator);
					string[][] commands = new string[switches.Length][];

					for (int i = 0; i < switches.Length; i++)
						commands[i] = switches[i].Split(commandSeparator);

					newHotkey = new HotkeySwitch(hotkey.key, commands);
				}
				else
				{
					string[] commands = hotkey.command.Split(commandSeparator);
					newHotkey = new HotkeyCommand(hotkey.key, commands);
				}

				return newHotkey;
			}
		}


		static class CommandRunner
		{
			class  MultipleCommands: MonoBehaviour {} // for coroutines
			static MultipleCommands multCmdRunner;

			public static float waitTime = 0f; // for pause between consecutive commands

			public static void run(string[] commands)
			{
				if (commands.Length == 1)
				{
					run(commands[0]);
				}
				else
				{
					multCmdRunner ??= helperGameObject.ensureComponent<MultipleCommands>();
					multCmdRunner.StartCoroutine(_run(commands));
				}
			}

			static IEnumerator _run(string[] commands)
			{
				foreach (var cmd in commands)
				{
					run(cmd);

					yield return waitTime > 0f? new WaitForSeconds(waitTime): null;
					waitTime = 0f;
				}
			}

			static void run(string command) => DevConsole.SendConsoleCommand(command);
		}


		static List<Hotkey> hotkeys;
		static GameObject helperGameObject;

		class HotkeyUpdateLoop: MonoBehaviour
		{
			void Update() => hotkeys.ForEach(hk => hk.update());
		}

		public static void updateBinds() => HotkeyInitializer.updateBinds();

		public static void wait(float secs) => CommandRunner.waitTime = secs;

		public static void setKeys(List<ModConfig.Hotkey> keys)
		{
			helperGameObject ??= UnityHelper.createPersistentGameObject<HotkeyUpdateLoop>("HotkeyHelper");

			hotkeys = HotkeyInitializer.create(keys);
		}
	}
}