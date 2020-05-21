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
		static bool Prepare() => Main.config.disableDevTools;

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

	/// disable F6
	//[HarmonyPatch(typeof(GUIController), "Update")]
	//class GUIController_Update_Patch
	//{
	//	static bool Prefix()
	//	{
	//		return !Main.config.disableDevTools;
	//	}
	//}

	/// disable F8
	//[HarmonyPatch(typeof(uGUI_FeedbackCollector), "Awake")]
	//class uGUI_FeedbackCollector_Update_Patch
	//{
	//	static void Postfix(uGUI_FeedbackCollector __instance)
	//	{
	//		__instance.enabled = false;
	//	}
	//}
}