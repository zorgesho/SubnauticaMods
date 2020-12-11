using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;

namespace CustomHotkeys
{
	using Debug = Common.Debug;

	[PatchClass]
	static class HotkeyHelper
	{
		abstract class Hotkey
		{
			protected Hotkey(KeyWithModifier key, bool up, bool held)
			{
				this.key = key;

				if (up)   targetState |= GameInput.InputStateFlags.Up;
				if (held) targetState |= GameInput.InputStateFlags.Held;
			}

			readonly GameInput.InputStateFlags targetState = GameInput.InputStateFlags.Down;

			public KeyWithModifier key
			{
				get => _key;
				set
				{
					_key = value;

					keyInputIndex = getInputIndex(_key.key);
					modInputIndex = getInputIndex(_key.modifier);

					modIndexes ??= initModIndexes();

					if (modIndexes != null)
						isJustModifier = modIndexes.contains(keyInputIndex);
				}
			}
			KeyWithModifier _key;

			int keyInputIndex, modInputIndex;
			bool isJustModifier = false; // is hotkey is just one modifier

			static int getInputIndex(KeyCode keyCode)
			{
				if (keyCode == KeyCode.None || GameInput.inputs.Count == 0)
					return -1;

				return GameInput.inputs.FindIndex(input => input.keyCode == keyCode);
			}

			static int[] modIndexes;
			static int[] initModIndexes()
			{
				if (GameInput.inputs.Count == 0)
					return null;

				return KeyWithModifier.modifiers.Select(keyCode => getInputIndex(keyCode)).ToArray();
			}

			bool getModState() // check that only necessary modifier is held down
			{
				Debug.assert(modIndexes != null);

				int heldIndex = isJustModifier? keyInputIndex: modInputIndex;

				foreach (var index in modIndexes)
				{
					if (index != heldIndex)
					{
						if (getKeyState(index) != 0u) // checking other modifiers
							return false;
					}
					else if (!isJustModifier && (getKeyState(index) & GameInput.InputStateFlags.Held) == 0u)
					{
						return false;
					}
				}

				return true;
			}

			static GameInput.InputStateFlags getKeyState(int index)
			{
				return index == -1? 0u: GameInput.inputStates[index].flags;
			}

			bool getKeyState()
			{
				 if (key.key == KeyCode.None || GameInput.clearInput || GameInput.scanningInput || FPSInputModule.current?.lockMovement == true)
					return false;

				return (getKeyState(keyInputIndex) & targetState) != 0u && getModState();
			}

			public void update()
			{
				if (getKeyState())
					runCmd();
			}

			protected abstract void runCmd();
		}


		class HotkeyCommand: Hotkey
		{
			readonly string[] commands;

			public HotkeyCommand(KeyWithModifier key, bool up, bool held, string[] commands):
				base(key, up, held) => this.commands = commands;

			protected override void runCmd() => CommandRunner.run(commands);
		}

		class HotkeySwitch: Hotkey
		{
			int index;
			readonly string[][] commands;

			public HotkeySwitch(KeyWithModifier key, bool up, bool held, string[][] commands):
				base(key, up, held) => this.commands = commands;

			protected override void runCmd() => CommandRunner.run(commands[index++ % commands.Length]);
		}


		static class HotkeyInitializer
		{
			const char switchSeparator = '|';
			const char commandSeparator = ';';

			static List<Tuple<Hotkey, HKConfig.Hotkey>> keysInfo;

			public static void updateBinds() => keysInfo.ForEach(info => info.Item1.key = info.Item2.key);

			public static List<Hotkey> create(List<HKConfig.Hotkey> keys)
			{
				keysInfo = keys.Select(hk => Tuple.Create(create(hk), hk)).ToList();
				return keysInfo.Select(hk => hk.Item1).ToList();
			}

			static Hotkey create(HKConfig.Hotkey hotkey)
			{
				Hotkey newHotkey;

				bool up = hotkey.mode == HKConfig.Hotkey.Mode.PressRelease;
				bool hold = hotkey.mode == HKConfig.Hotkey.Mode.Hold;

				if (hotkey.command.Contains(switchSeparator))
				{
					string[]   switches = hotkey.command.Split(switchSeparator);
					string[][] commands = new string[switches.Length][];

					for (int i = 0; i < switches.Length; i++)
						commands[i] = switches[i].Split(commandSeparator);

					newHotkey = new HotkeySwitch(hotkey.key, up, hold, commands);
				}
				else
				{
					string[] commands = hotkey.command.Split(commandSeparator);
					newHotkey = new HotkeyCommand(hotkey.key, up, hold, commands);
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
				Debug.assert(commands != null);

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


		[HarmonyPostfix, HarmonyPatch(typeof(GameInput), "Initialize")]
		static void setInputIndexes() => hotkeys?.ForEach(hk => hk.key = hk.key); // reassign keys to update input indexes

		static List<Hotkey> hotkeys;
		static GameObject helperGameObject;

		class HotkeyUpdateLoop: MonoBehaviour
		{
			void Update() => hotkeys.ForEach(hk => hk.update());
		}

		public static void updateBinds() => HotkeyInitializer.updateBinds();

		public static void wait(float secs) => CommandRunner.waitTime = secs;

		public static void setKeys(List<HKConfig.Hotkey> keys)
		{
			helperGameObject ??= UnityHelper.createPersistentGameObject<HotkeyUpdateLoop>("HotkeyHelper");

			hotkeys = HotkeyInitializer.create(keys);
		}
	}
}