using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Common;
using Common.Reflection;

namespace MiscPrototypes
{
	class TestConsoleCommands: PersistentConsoleCommands
	{
		void debug_raycasters()
		{
			var raycasterManager = Type.GetType("UnityEngine.EventSystems.RaycasterManager, UnityEngine.UI");
			var raycasterList = raycasterManager.field("s_Raycasters");

			foreach (var raycaster in raycasterList.GetValue(null) as List<UnityEngine.EventSystems.BaseRaycaster>)
				raycaster.ToString().log();
		}

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