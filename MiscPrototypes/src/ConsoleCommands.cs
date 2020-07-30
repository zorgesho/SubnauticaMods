using System.Text;
using System.Collections;

using Harmony;
using UnityEngine;

using Common;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(GameInput), "Initialize")] // just to register console commands
	static class GameInput_Awake_Patch_ConsoleCommands
	{
		static void Postfix() => PersistentConsoleCommands.register<TestConsoleCommands>();
	}

	class TestConsoleCommands: PersistentConsoleCommands
	{
		void debug_gameinput(bool enable = false)
		{
			StopAllCoroutines();

			if (enable)
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

		void debug_print_gameinput()
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