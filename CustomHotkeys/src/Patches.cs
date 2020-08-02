using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.EventSystems;

using Common;
using Common.Harmony;
using Common.Reflection;
using Common.Configuration;

namespace CustomHotkeys
{
	using Debug = Common.Debug;
	using CIEnumerable = IEnumerable<CodeInstruction>;

	[OptionalPatch, PatchClass]
	static class DevToolsHotkeysPatch
	{
		static bool prepare() => !Main.config.enableDevToolsHotkeys;

		// disabling F1 and F3 hotkeys for dev tools
		[HarmonyTranspiler, HarmonyPatch(typeof(MainGameController), "Update")]
		static CIEnumerable F1_F3_disabler(CIEnumerable cins)
		{
			var list = cins.ToList();
			var isShipping = typeof(PlatformUtils).method("get_isShippingRelease");

			int indexBegin = list.FindIndex(ci => ci.isOp(OpCodes.Call, isShipping));
			int indexEnd   = list.FindLastIndex(ci => ci.isOp(OpCodes.Blt)) + 1;
			Debug.assert(indexBegin != -1 && indexEnd != 0);

			return CIHelper.ciRemove(list, indexBegin, indexEnd - indexBegin);
		}

		// disable F6 (hide gui tool)
		[HarmonyPrefix, HarmonyPatch(typeof(GUIController), "Update")]
		static bool F6_disabler() => false;
	}

	[OptionalPatch, PatchClass]
	static class FeedbackCollectorPatch
	{
		static bool prepare() => !Main.config.enableFeedback;

		public class SettingChanged: Config.Field.IAction
		{
			public void action()
			{
				if (uGUI_FeedbackCollector.main)
					uGUI_FeedbackCollector.main.enabled = Main.config.enableFeedback;
			}
		}

		// disable F8 (feedback collector)
		[HarmonyPostfix, HarmonyPatch(typeof(uGUI_FeedbackCollector), "Awake")]
		static void uGUIFeedbackCollector_Awake_Postfix(uGUI_FeedbackCollector __instance) => __instance.enabled = Main.config.enableFeedback;

		// remove "Give Feedback" from the ingame menu
		[HarmonyTranspiler, HarmonyPatch(typeof(IngameMenu), "Start")]
		static CIEnumerable IngameMenu_Start_Transpiler(CIEnumerable cins) => CIHelper.ciRemove(cins, 0, 3);
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

			static CIEnumerable Transpiler(CIEnumerable cins)
			{
				var list = cins.ToList();

				// saving binded keycode to check later in GameInput.UpdateKeyInputs patch
				var GameInput_ClearInput = typeof(GameInput).method("ClearInput");
				CIHelper.ciInsert(list, ci => ci.isOp(OpCodes.Call, GameInput_ClearInput), CIHelper.emitCall<Action>(saveLastBind));

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

		// if we press key while binding in options menu, ignore its 'Up' & 'Held' events
		[HarmonyPatch(typeof(GameInput), "UpdateKeyInputs")]
		static class GameInput_UpdateKeyInputs_Patch
		{
			static CIEnumerable Transpiler(CIEnumerable cins, ILGenerator ilg)
			{
				var list = cins.ToList();

				var Input_GetKey = typeof(Input).method("GetKey", typeof(KeyCode));
				var Input_GetKeyUp = typeof(Input).method("GetKeyUp", typeof(KeyCode));
				var field_lastBindedIndex = typeof(BindingPatches).field(nameof(lastBindedIndex));

				int index = list.FindIndex(ci => ci.isOp(OpCodes.Call, Input_GetKey));
				Debug.assert(index != -1);

				Label lb0 = list[index + 1].operand.cast<Label>();

				list.InsertRange(index + 2, CIHelper.toCIList
				(
					OpCodes.Ldloc_S, 4,						//	if (i == BindingPatches.lastBindedIndex)
					OpCodes.Ldsfld, field_lastBindedIndex,
					OpCodes.Beq_S, lb0
				));

				index = list.FindIndex(ci => ci.isOp(OpCodes.Call, Input_GetKeyUp));
				Debug.assert(index != -1);

				Label lb1 = list[index + 1].operand.cast<Label>();
				Label lb2 = ilg.DefineLabel();
				list[index + 2].labels.Add(lb2); // label for 'inputState.flags |= GameInput.InputStateFlags.Up'

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