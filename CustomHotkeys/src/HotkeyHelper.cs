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
				if (checkHotkey())
					activate();
			}

			bool checkHotkey() // TODO hold mode && check for unpressed modifiers
			{
				return KeyCodeUtils.GetKeyDown(key.key) && (key.modifier == KeyCode.None || KeyCodeUtils.GetKeyHeld(key.modifier));
			}

			protected abstract void activate();
		}

		class HotkeyCommand: Hotkey
		{
			readonly string[] commands;

			public HotkeyCommand(InputHelper.KeyWithModifier key, string[] commands): base(key) => this.commands = commands;

			protected override void activate()
			{
				CommandRunner.run(commands);
			}
		}

		class HotkeySwitch: Hotkey
		{
			int index;
			readonly string[][] commands;

			public HotkeySwitch(InputHelper.KeyWithModifier key, string[][] commands): base(key) => this.commands = commands;

			protected override void activate()
			{
				CommandRunner.run(commands[index++ % commands.Length]);
			}
		}

		static List<Hotkey> hotkeys;

		public static void updateKeys() => ConfigParser.updateKeys();

		static float waitTime = 0f; // for use in console commands
		public static void setWaitTime(float t) => waitTime = t;


		static class ConfigParser
		{
			const char switchSeparator = '|';
			const char commandSeparator = ';';

			public static Dictionary<ModConfig.Hotkey, Hotkey> configKeys = new Dictionary<ModConfig.Hotkey, Hotkey>();

			public static void updateKeys()
			{
				foreach (var link in configKeys)
					link.Value.key = link.Key.key;
			}

			public static Hotkey create(ModConfig.Hotkey hotkey)
			{
				Hotkey res;

				if (hotkey.command.Contains(switchSeparator))
				{
					string[]   switches = hotkey.command.Split(switchSeparator);
					string[][] commands = new string[switches.Length][];

					for (int i = 0; i < switches.Length; i++)
						commands[i] = switches[i].Split(commandSeparator);

					res = new HotkeySwitch(hotkey.key, commands);
				}
				else
				{
					string[] commands = hotkey.command.Split(commandSeparator);
					res = new HotkeyCommand(hotkey.key, commands);
				}

				configKeys[hotkey] = res;
				return res;
			}
		}


		static class CommandRunner
		{
			class MultipleCommands: MonoBehaviour {} // for coroutines

			static MultipleCommands multCmdRunner;

			public static void init()
			{
				multCmdRunner = hotkeyHelperGO.ensureComponent<MultipleCommands>();
			}

			static IEnumerator _run(string[] commands)
			{
				foreach (var cmd in commands)
				{
					run(cmd);

					yield return waitTime > 0? new WaitForSeconds(waitTime): null;
					waitTime = 0f;
				}
			}

			public static void run(string command)
			{
				DevConsole.SendConsoleCommand(command);
			}

			public static void run(string[] commands)
			{
				if (commands.Length == 1)
					run(commands[0]);
				else
					multCmdRunner.StartCoroutine(_run(commands));
			}
		}

		class HotkeyListener: MonoBehaviour
		{
			void Update() => hotkeys.ForEach(hotkey => hotkey.update());
		}

		static GameObject hotkeyHelperGO;

		public static void init(List<ModConfig.Hotkey> hotkeys)
		{
			if (hotkeyHelperGO)
				return;

			hotkeyHelperGO = UnityHelper.createPersistentGameObject<HotkeyListener>("HotkeyHelper");
			CommandRunner.init();

			HotkeyHelper.hotkeys = hotkeys.Select(hk => ConfigParser.create(hk)).ToList();
		}
	}
}