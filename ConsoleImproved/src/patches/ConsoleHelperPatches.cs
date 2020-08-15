using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common.Harmony;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		[PatchClass]
		static class Patches
		{
			// patch for full history in console
			[HarmonyTranspiler, HarmonyPatch(typeof(ConsoleInput), "Validate")]
			static IEnumerable<CodeInstruction> ConsoleInput_Validate_Transpiler(IEnumerable<CodeInstruction> cins) =>
				CIHelper.ciRemove(cins, 0, 5); // remove first line "this.historyIndex = this.history.Count;"

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

				__result = true;
				return false;
			}
		}
	}
}