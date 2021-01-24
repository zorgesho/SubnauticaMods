using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common.Harmony;

#if GAME_BZ
using System.Reflection.Emit;
using Common.Reflection;
#endif

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		[PatchClass]
		static class Patches
		{
#if GAME_SN
			// patch for full history in console
			[HarmonyTranspiler, HarmonyPatch(typeof(ConsoleInput), "Validate")]
			static IEnumerable<CodeInstruction> ConsoleInput_Validate_Transpiler(IEnumerable<CodeInstruction> cins) =>
				CIHelper.ciRemove(cins, 0, 5); // remove first line "this.historyIndex = this.history.Count;"
#endif
			[HarmonyPostfix, HarmonyPatch(typeof(DevConsole), "Awake")]
			static void DevConsole_Awake_Postfix()
			{
				init();
				loadHistory();
			}

			[HarmonyPostfix, HarmonyPatch(typeof(DevConsole), "OnDisable")]
			static void DevConsole_OnDisable_Postfix() => saveHistory();

			[HarmonyPrefix, HarmonyPatch(typeof(ConsoleInput), "KeyPressedOverride")]
			static bool ConsoleInput_KeyPressedOverride_Prefix(ConsoleInput __instance, ref bool __result)
			{
				KeyCode keyCode = __instance.processingEvent.keyCode;

				if (keyCode != KeyCode.Tab || __instance.text.Length == 0 || __instance.caretPosition != __instance.text.Length)
					return true;

				string ret = tryCompleteText(__instance.text);

				if (ret != "")
					__instance.text = ret;

				__instance.caretPosition = __instance.text.Length;

				__result = true;
				return false;
			}
#if GAME_BZ
			// don't select all text when going up/down
			[HarmonyTranspiler]
			[HarmonyPatch(typeof(ConsoleInput), "ProcessUpKey")]
			[HarmonyPatch(typeof(ConsoleInput), "ProcessDownKey")]
			static IEnumerable<CodeInstruction> ConsoleInput_ProcessUpDownKey_Transpiler(IEnumerable<CodeInstruction> cins)
			{
				var selectAll = typeof(TMPro.TMP_InputField).method("SelectAll");
				return CIHelper.ciRemove(cins, cin => cin.isOp(OpCodes.Call, selectAll), -1, 2); // remove call to SelectAll
			}

			// put caret to the end of text when going up/down
			[HarmonyPostfix]
			[HarmonyPatch(typeof(ConsoleInput), "ProcessUpKey")]
			[HarmonyPatch(typeof(ConsoleInput), "ProcessDownKey")]
			static void ConsoleInput_ProcessUpDownKey_Postfix(ConsoleInput __instance)
			{
				__instance.caretPosition = __instance.text.Length;
			}
#endif
		}
	}
}