using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using Harmony;

namespace ConsoleImproved
{
	// don't clear onscreen messages while console is open
	[HarmonyPatch(typeof(ErrorMessage), "Update")]
	static class ErrorMessage_Update_Patch
	{
		static bool Prefix(ErrorMessage __instance)
		{
			return !(Main.config.keepMessagesOnScreen && DevConsole.instance.state);
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
			static void Postfix(DevConsole __instance)
			{
				init();
				loadHistory();
			}
		}


		[HarmonyPatch(typeof(DevConsole), "OnDisable")]
		static class DevConsole_OnDisable_Patch
		{
			static void Postfix(DevConsole __instance)
			{
				saveHistory();
			}
		}


		[HarmonyPatch(typeof(ConsoleInput), "KeyPressedOverride")]
		static class ConsoleInput_KeyPressedOverride_Patch
		{
			static bool Prefix(ConsoleInput __instance, Event evt, ref bool __result)
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