using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.EventSystems;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace CustomHotkeys
{
	using Debug = Common.Debug;

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

	// allows to remove bindings from bind options without selecting them first
	// it's enough to just move cursor over the option and press 'Delete'
	static class BindRemover
	{
		class BindCheckPointer: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
		{
			public static GameObject hoveredObject { get; private set; }

			public void OnPointerEnter(PointerEventData eventData) => hoveredObject = gameObject;
			public void OnPointerExit(PointerEventData eventData)  => hoveredObject = null;
		}

		[HarmonyPatch(typeof(uGUI_Binding), "Start")]
		static class uGUIBinding_Start_Patch
		{
			static bool Prepare() => Main.config.easyBindRemove;

			static void Postfix(uGUI_Binding __instance) =>
				__instance.gameObject.ensureComponent<BindCheckPointer>();
		}

		[HarmonyPatch(typeof(uGUI_Binding), "Update")]
		static class uGUIBinding_Update_Patch
		{
			static bool Prepare() => Main.config.easyBindRemove;

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
			{
				var list = cins.ToList();

				var toReplace = typeof(EventSystem).method("get_current");
				int index = list.FindIndex(ci => ci.isOp(OpCodes.Call, toReplace));
				Debug.assert(index != -1);

				CIHelper.ciRemove(list, index, 2);
				CIHelper.ciInsert(list, index,
					CIHelper.toCIList(OpCodes.Call, typeof(BindCheckPointer).method("get_" + nameof(BindCheckPointer.hoveredObject))));

				return list;
			}
		}
	}
}