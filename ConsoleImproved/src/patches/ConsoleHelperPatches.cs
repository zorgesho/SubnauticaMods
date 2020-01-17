using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		// patch for full history in console
		[HarmonyPatch(typeof(ConsoleInput), "Validate")]
		static class ConsoleInput_Validate_Patch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) =>
				HarmonyHelper.ciRemove(cins, 0, 5); // remove first line "this.historyIndex = this.history.Count;"
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