using System.Linq;
using System.Collections;

using UnityEngine;
using SMLHelper.V2.Utility;

using Common;

namespace CustomHotkeys
{
	class HotkeyListener
	{
		static IEnumerator runCommand(string command)
		{
			foreach (var cmd in command.Split(';'))
			{
				DevConsole.SendConsoleCommand(cmd);
				yield return null;
			}
		}

		class Listener: MonoBehaviour
		{
			void Update()
			{
				foreach (var hotkey in Main.config.hotkeys)
				{
					if (KeyCodeUtils.GetKeyDown(hotkey.key))
					{
						if (hotkey.command.Contains(';'))
							StartCoroutine(runCommand(hotkey.command));
						else
							DevConsole.SendConsoleCommand(hotkey.command);
					}
				}
			}
		}

		static bool inited = false;

		public static void init()
		{
			if (!inited || (inited = true))
				UnityHelper.createPersistentGameObject<Listener>("CustomHotkeys");
		}
	}
}