using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace CustomHotkeys
{
	// disabling F1 and F3 hotkeys for dev tools
	[OptionalPatch, HarmonyPatch(typeof(MainGameController), "Update")]
	static class MainGameController_Update_Patch
	{
		static bool Prepare() => !Main.config.enableDevToolsHotkeys;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();
			var isShipping = typeof(PlatformUtils).method("get_isShippingRelease");

			int indexBegin = list.FindIndex(ci => ci.isOp(OpCodes.Call, isShipping));
			int indexEnd   = list.FindLastIndex(ci => ci.isOp(OpCodes.Blt)) + 1;
			Debug.assert(indexBegin != -1 && indexEnd != 0);

			return CIHelper.ciRemove(list, indexBegin, indexEnd - indexBegin);
		}
	}

	// disable F6 (hide gui tool)
	[OptionalPatch, HarmonyPatch(typeof(GUIController), "Update")]
	static class GUIController_Update_Patch
	{
		static bool Prepare() => !Main.config.enableDevToolsHotkeys;
		static bool Prefix() => false;
	}

	// disable F8 (feedback collector)
	[HarmonyPatch(typeof(uGUI_FeedbackCollector), "Awake")]
	static class uGUIFeedbackCollector_Awake_Patch
	{
		static void Postfix(uGUI_FeedbackCollector __instance) => __instance.enabled = Main.config.enableFeedback;
	}
}