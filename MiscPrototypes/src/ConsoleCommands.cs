using System;
using System.Collections;
using System.Text;

using Harmony;
using UnityEngine;

using Common;
using Common.Reflection;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(GameInput), "Initialize")] // just to create console commands object
	static class GameInput_Awake_Patch_ConsoleCommands
	{
#pragma warning disable IDE0052
		static GameObject go = null;
#pragma warning restore
		static void Postfix() => go ??= PersistentConsoleCommands.createGameObject<TestConsoleCommands>();
	}


	class TestConsoleCommands: PersistentConsoleCommands
	{
		void OnConsoleCommand_debug_gameinput(NotificationCenter.Notification n)
		{
			StopAllCoroutines();

			if (n.getArg<bool>(0))
				StartCoroutine(_dbg());

			static IEnumerator _dbg()
			{
				while (true)
				{
					var sb = new StringBuilder();
					sb.AppendLine();
					sb.AppendLine($"clearInput: {GameInput.clearInput}; scanningInput: {GameInput.scanningInput}");
					sb.AppendLine($"movedir: {GameInput.GetMoveDirection()}");
					sb.AppendLine($"playercontroller: {Player.main?.playerController.inputEnabled}");
					sb.AppendLine($"fpsmod lock: {FPSInputModule.current.lockMovement}");

					foreach (var mod in InputHelper.KeyWithModifier.modifiers)
					{
						sb.AppendLine($"{mod}: down: {Input.GetKeyDown(mod)} up: {Input.GetKeyUp(mod)} held: {Input.GetKey(mod)}");
					}

					sb.ToString().onScreen("gameinput");
					yield return null;
				}
			}
		}


		void OnConsoleCommand_debug_print_gameinput(NotificationCenter.Notification n)
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			for (int i = 0; i < GameInput.inputs.Count; i++)
			{
				var input = GameInput.inputs[i];
				sb.AppendLine($"{input.name} {(int)input.keyCode} {input.axis} {input.axisPositive}");
			}

			sb.ToString().saveToFile("gameinput");
		}
	}
}