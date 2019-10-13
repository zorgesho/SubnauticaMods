using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

namespace ConsoleImproved
{
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
	}


	//private bool KeyPressedOverride(Event evt)
	//[HarmonyPatch(typeof(ConsoleInput), "KeyPressedOverride")]
	//static class DevConsole_Awake_Patch11
	//{
	//	static bool Prefix(ConsoleInput __instance, Event evt, ref bool __result)
	//	{
	//		KeyCode keyCode = __instance.processingEvent.keyCode;

	//		if (keyCode == KeyCode.Tab)
	//		{
	//			"TAB".onScreen();
	//			__instance.text += "ololo";//.onScreen();
	//		}

	//		return true;
	//	}
	//}
}