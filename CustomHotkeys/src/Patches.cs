using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;
using Common.Reflection;
using Common.Configuration;

#if GAME_SN
using UnityEngine.EventSystems;
#endif

namespace CustomHotkeys
{
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
			var checkProp = typeof(PlatformUtils).method(Mod.Consts.isGameSN? "get_isShippingRelease": "get_isConsolePlatform");

			int[] i = list.ciFindIndexes(ci => ci.isOp(OpCodes.Call, checkProp),
										 ci => ci.isOp(OpCodes.Call, checkProp),
										 ci => ci.isOp(Mod.Consts.isGameSN? OpCodes.Blt: OpCodes.Ret));

			return i == null? cins: list.ciRemoveRange(i[0], i[2]);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GUIController), "Update")] // disable F6 (hide gui tool)
		[HarmonyPatch(typeof(MainMenuController), "Update")] // disable Shift+F5 (smoke test)
		static bool hotkeyDisabler() => false;
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
#if GAME_SN
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
#endif
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
#if GAME_SN
				if (Main.config.easyBindRemove)
				{
					var toReplace = typeof(EventSystem).method("get_current");
					int index = list.FindIndex(ci => ci.isOp(OpCodes.Call, toReplace));
					Common.Debug.assert(index != -1);

					var get_hoveredObject = typeof(BindCheckPointer).method("get_" + nameof(BindCheckPointer.hoveredObject));
					list.RemoveRange(index, 2);
					list.Insert(index, new CodeInstruction(OpCodes.Call, get_hoveredObject));
				}
#endif
				return list;
			}
		}

#if GAME_BZ
		[HarmonyPatch(typeof(uGUI_Binding), "RefreshValue")]
		static class uGUIBinding_RefreshValue_Patch
		{
			static bool Prefix(uGUI_Binding __instance)
			{
				if (!__instance.gameObject.GetComponent<KeyWModBindOption.Tag>())
					return true;

				__instance.currentText.text = (__instance.active || __instance.value == null)? "": __instance.value;
				__instance.UpdateState();
				return false;
			}
		}
#endif
		// if we press key while binding in options menu, ignore its 'Up' & 'Held' events
		[HarmonyPatch(typeof(GameInput), Mod.Consts.isGameSNStable? "UpdateKeyInputs": "GetInputState")]
		static class GameInput_UpdateKeyState_Patch
		{
			static CIEnumerable Transpiler(CIEnumerable cins, ILGenerator ilg)
			{
				var list = cins.ToList();
				var field_lastBindedIndex = typeof(BindingPatches).field(nameof(lastBindedIndex));
#if GAME_SN && BRANCH_STABLE
				var Input_GetKey = typeof(Input).method("GetKey", typeof(KeyCode));
				var cinsCompare = CIHelper.toCIList(OpCodes.Ldloc_S, 4,
													OpCodes.Ldsfld, field_lastBindedIndex);
#else
				object Input_GetKey = null; // exp branch uses InputUtils, but we don't really need to check method in GetInputState
				var cinsCompare = CIHelper.toCIList(OpCodes.Ldarg_1, CIHelper.emitCall<Func<KeyCode>>(_lastBindedKeyCode));

				static KeyCode _lastBindedKeyCode() =>
					lastBindedIndex == -1 || GameInput.inputs.Count == 0? default: GameInput.inputs[lastBindedIndex].keyCode;
#endif
				int[] i = list.ciFindIndexes(ci => ci.isOp(OpCodes.Call, Input_GetKey),
											 ci => ci.isOp(OpCodes.Call),
											 ci => ci.isOp(OpCodes.Call));
				if (i == null)
					return cins;

				Label lb1 = list[i[2] + 1].operand.cast<Label>();
				Label lb2 = list.ciDefineLabel(i[2] + 2, ilg); // label for 'inputState.flags |= GameInput.InputStateFlags.Up'

				CIHelper.LabelClipboard.__enabled = false;
				list.ciInsert(i[2] + 2,
					cinsCompare,							// compare last binded key with current
					OpCodes.Bne_Un_S, lb2,
					OpCodes.Ldc_I4_M1,						// BindingPatches.lastBindedIndex = -1;
					OpCodes.Stsfld, field_lastBindedIndex,
					OpCodes.Br_S, lb1);						// else inputState.flags |= GameInput.InputStateFlags.Up;

				Label lb0 = list[i[0] + 1].operand.cast<Label>();
				list.ciInsert(i[0] + 2, cinsCompare, OpCodes.Beq_S, lb0);

				return list;
			}
		}
	}

#if GAME_SN // doesn't needed for BZ
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
#endif
}