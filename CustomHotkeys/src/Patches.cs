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

	// patches for removing bindings and blocking 'Up' event after binding
	static class BindingPatches
	{
		class BindCheckPointer: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
		{
			public static GameObject hoveredObject { get; private set; }

			public void OnPointerEnter(PointerEventData eventData) => hoveredObject = gameObject;
			public void OnPointerExit(PointerEventData eventData)  => hoveredObject = null;
		}

		// allows to remove bindings from bind options without selecting them first
		// it's enough to just move cursor over the option and press 'Delete'
		// another part of the patch in the 'Update' patch
		[HarmonyPatch(typeof(uGUI_Binding), "Start")]
		static class uGUIBinding_Start_Patch
		{
			static bool Prepare() => Main.config.easyBindRemove;

			static void Postfix(uGUI_Binding __instance) =>
				__instance.gameObject.ensureComponent<BindCheckPointer>();
		}

		static int lastBindedIndex = -1;

		[HarmonyPatch(typeof(uGUI_Binding), "Update")]
		static class uGUIBinding_Update_Patch
		{
			static void saveLastBind() => lastBindedIndex = GameInput.lastInputPressed[0]; // for keyboard

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
			{
				var list = cins.ToList();

				// saving binded keycode to check later in GameInput.UpdateKeyInputs patch
				var GameInput_ClearInput = typeof(GameInput).method("ClearInput");
				CIHelper.ciInsert(list, ci => ci.isOp(OpCodes.Call, GameInput_ClearInput),
					OpCodes.Call, typeof(uGUIBinding_Update_Patch).method(nameof(saveLastBind)));

				if (Main.config.easyBindRemove)
				{
					var toReplace = typeof(EventSystem).method("get_current");
					int index = list.FindIndex(ci => ci.isOp(OpCodes.Call, toReplace));
					Debug.assert(index != -1);

					var get_hoveredObject = typeof(BindCheckPointer).method("get_" + nameof(BindCheckPointer.hoveredObject));
					list.RemoveRange(index, 2);
					list.Insert(index, new CodeInstruction(OpCodes.Call, get_hoveredObject));
				}

				return list;
			}
		}

		// if we press key while binding in options menu, ignore its 'Up' event too
		[HarmonyPatch(typeof(GameInput), "UpdateKeyInputs")]
		static class GameInput_UpdateKeyInputs_Patch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
			{
				var list = cins.ToList();

				var Input_GetKeyUp = typeof(Input).method("GetKeyUp", typeof(KeyCode));
				int index = list.FindIndex(ci => ci.isOp(OpCodes.Call, Input_GetKeyUp));
				Debug.assert(index != -1);

				Label lb1 = list[index + 1].operand.cast<Label>();
				Label lb2 = ilg.DefineLabel();
				list[index + 2].labels.Add(lb2); // label for 'inputState.flags |= GameInput.InputStateFlags.Up'

				var field_lastBindedIndex = typeof(BindingPatches).field(nameof(lastBindedIndex));

				list.InsertRange(index + 2, CIHelper.toCIList
				(
					OpCodes.Ldloc_S, 4,						//	if (i == BindingPatches.lastBindedIndex)
					OpCodes.Ldsfld, field_lastBindedIndex,
					OpCodes.Bne_Un_S, lb2,
					OpCodes.Ldc_I4_M1,						//		BindingPatches.lastBindedIndex = -1;
					OpCodes.Stsfld, field_lastBindedIndex,
					OpCodes.Br_S, lb1						//	else inputState.flags |= GameInput.InputStateFlags.Up;
				));

				return list;
			}
		}
	}

	static class GameInput_AutoForward_Patch
	{
		static bool patched = false;
		static bool autoforward = false;

		public static void setAutoForward(bool val)
		{
			if (!patched && (patched = true))
				HarmonyHelper.patch();

			autoforward = val;
		}

		public static void toggleAutoForward() => setAutoForward(!autoforward);

		[HarmonyPostfix, HarmonyPatch(typeof(GameInput), "GetMoveDirection")]
		static void patchAutoForward(ref Vector3 __result)
		{
			if (autoforward)
				__result.z = 1f;
		}
	}
}