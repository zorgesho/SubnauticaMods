using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Harmony;

namespace ConsoleImproved
{
	// don't clear onscreen messages while console is open
	//[HarmonyPatch(typeof(ErrorMessage), "Update")]
	//static class ErrorMessage_Update_Patch
	//{
	//	static bool Prefix()
	//	{
	//		return !(Main.config.keepMessagesOnScreen && DevConsole.instance.state);
	//	}
	//}

	[HarmonyPatch(typeof(ErrorMessage), "OnUpdate")]
	static class ErrorMessage_Update_Patch
	{
		static bool Prefix(ErrorMessage __instance)
		{
			float time = Time.time;
			
			if (!DevConsole.instance.state)
				return true;

			//for (int i = 0; i < __instance.messages.Count; i++)
			//{
			//	ErrorMessage._Message message = __instance.messages[i];
			//	if (time > message.timeEnd)
			//	{
			//		__instance.offsetY += message.entry.preferredHeight;
			//		__instance.messages.Remove(message);
			//		__instance.ReleaseEntry(message.entry);
			//	}
			//}

			float num = __instance.offsetY * 7f;
			__instance.offsetY -= num * Time.deltaTime;
			if (__instance.offsetY < 1f)
			{
				__instance.offsetY = 0f;
			}
			Rect rect = __instance.messageCanvas.rect;
			for (int j = 0; j < __instance.messages.Count; j++)
			{
				ErrorMessage._Message message2 = __instance.messages[j];
				Text entry = message2.entry;
				RectTransform rectTransform = entry.rectTransform;
				Vector2 a = new Vector2(-0.5f * Mathf.Min(rectTransform.rect.width, entry.preferredWidth), 0.5f * entry.preferredHeight);
				Vector2 b = new Vector2(rect.x + __instance.offset.x, -rect.y + __instance.GetYPos(j));
				float value = Mathf.Clamp01(MathExtensions.EvaluateLine(message2.timeEnd - __instance.timeInvisible - __instance.timeFadeOut - __instance.timeDelay - __instance.timeFlyIn, 0f, message2.timeEnd - __instance.timeInvisible - __instance.timeFadeOut - __instance.timeDelay, 1f, time));
				rectTransform.localPosition = Vector2.Lerp(a, b, MathExtensions.EaseOutSine(value));
				
				//float value2 = Mathf.Clamp01(MathExtensions.EvaluateLine(message2.timeEnd - __instance.timeInvisible - __instance.timeFadeOut, 1f, message2.timeEnd - __instance.timeInvisible, 0f, time));
				//entry.canvasRenderer.SetAlpha(MathExtensions.EaseOutSine(value2));
			}

			return false;
		}
	}

	static partial class ConsoleHelper
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
			static void Postfix()
			{
				init();
				loadHistory();
			}
		}


		[HarmonyPatch(typeof(DevConsole), "OnDisable")]
		static class DevConsole_OnDisable_Patch
		{
			static void Postfix()
			{
				saveHistory();
			}
		}


		[HarmonyPatch(typeof(ConsoleInput), "KeyPressedOverride")]
		static class ConsoleInput_KeyPressedOverride_Patch
		{
			static bool Prefix(ConsoleInput __instance, ref bool __result)
			{
				KeyCode keyCode = __instance.processingEvent.keyCode;

				if (keyCode == KeyCode.Tab)
				{
					if (__instance.text.Length > 0 && __instance.caretPosition == __instance.text.Length)
					{
						string ret = tryCompleteText(__instance.text);

						if (ret != "")
							__instance.text = ret;

						__result = true;
						return false;
					}
				}

				return true;
			}
		}
	}
}