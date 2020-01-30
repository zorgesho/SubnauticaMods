using System;
using System.Text;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(UWE.Utils), "UpdateCusorLockState")] // just to create GuiWatcher object
	static class UpdateCusorLockState_Patch
	{
		static GameObject dbgObject = null;
		static bool Prepare()
		{
			if (!dbgObject)
			{
				dbgObject = PersistentConsoleCommands.createGameObject<ConsoleCommands>();
				dbgObject.AddComponent<GuiWatcher>();
			}
			return false;
		}
		static void Postfix()
		{
			"--------------- UpdateCusorLockState".log();
			Common.Debug.logStack();
		}
	}

	//[HarmonyPatch(typeof(UWE.FreezeTime), "Begin")]
	//static class FreezeTime_Begin_Patch
	//{
	//	static bool Prefix(string userId) => userId != "IngameMenu";
	//}

	//[HarmonyPatch(typeof(InputHandlerStack), "Push", new Type[] { typeof(GameObject) })]
	static class InputHandlerStack_Patch_Push
	{
		static void Postfix(GameObject handler)
		{
			$"InputHandlerStack PUSH {handler.name}".log();

			"--stack begin".log();
			foreach (var s in InputHandlerStack.main.stack.ToArray())
			{
				$"{s.name}".log();
			}
			"--stack end".log();
		}
	}

	//[HarmonyPatch(typeof(InputHandlerStack), "Pop", new Type[] { typeof(GameObject) })]
	static class InputHandlerStack_Patch_Pop
	{
		static void Postfix(GameObject handler)
		{
			$"InputHandlerStack POP {handler.name}".log();
			"--stack begin".log();
			foreach (var s in InputHandlerStack.main.stack.ToArray())
			{
				$"{s.name}".log();
			}
			"--stack end".log();
		}
	}



	//[HarmonyPatch(typeof(uGUI_InputGroup), "InterceptInput")]
	static class uGUIInputGroup_Patch_InterceptInput
	{
		static bool Prefix(uGUI_InputGroup __instance, bool state)
		{
			$"InterceptInput {__instance.gameObject.name} {state}".log();

			if (__instance.inputDummy.activeSelf == state)
			{
				"1".log();
				return false;
			}
			if (state)
			{
				InputHandlerStack.main.Push(__instance.inputDummy);
				__instance.cursorLockCached = UWE.Utils.lockCursor;
				UWE.Utils.lockCursor = false;
				"2".log();
				return false;
			}

			UWE.Utils.lockCursor = __instance.cursorLockCached;
			InputHandlerStack.main.Pop(__instance.inputDummy);
			"3".log();
			return false;
		}
	}

#if USER_INPUT_GROUP_PATCHES

	[HarmonyPatch(typeof(uGUI_UserInput), "Close")]
	static class uGUIUserInput_Patch_Close
	{
		static void Postfix(bool _submit)
		{
			$"CLOSE {_submit}".log();
		}
	}
	[HarmonyPatch(typeof(uGUI_UserInput), "OnEndEdit")]
	static class uGUIUserInput_Patch_OnEndEdit
	{
		static void Postfix()
		{
			"ON_END_EDIT".log();
			//Common.Debug.logStack();

		}
	}
	[HarmonyPatch(typeof(uGUI_UserInput), "OnSubmit")]
	static class uGUIUserInput_Patch_OnSubmit
	{
		static void Postfix()
		{
			"ON_SUBMIT".log();
		}
	}
	[HarmonyPatch(typeof(uGUI_UserInput), "SetState")]
	static class uGUIUserInput_Patch_SetState
	{
		static void Postfix(uGUI_UserInput __instance, bool newState)
		{
			if (false && !__instance.gameObject.GetComponent<uGUI_GraphicRaycaster>())
			{
				//CanvasGroup cmp_ = null;

				//uGUI_GraphicRaycaster cmp = __instance.gameObject.AddComponent<uGUI_GraphicRaycaster>();
				////cmp.copyFieldsFrom(IngameMenu.main.GetComponent<uGUI_GraphicRaycaster>());
				////__instance.gameObject.AddComponent<Canvas>();

				//__instance.gameObject.dump("input_raycasternew");

			}

			$"SET_STATE {newState}".log();
			//Common.Debug.logStack();
		}
	}
#endif

	class GuiWatcher: MonoBehaviour
	{
		readonly StringBuilder sb = new StringBuilder();

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.F5))
				DevConsole.SendConsoleCommand("test1");
			if (Input.GetKeyDown(KeyCode.F6))
			{
				UWE.Utils.lockCursor = true;
				UWE.Utils.lockCursor = false;
			}
			if (Input.GetKeyDown(KeyCode.F7))
			{
				Cursor.lockState = CursorLockMode.Locked;
			}

			if (true)
			{
				sb.Clear();
				sb.AppendLine("");

				foreach (var s in InputHandlerStack.main.stack.ToArray())
				{
					sb.AppendLine(s.name);
				}

				sb.ToString().onScreen("InputStack");

				if (FPSInputModule.current?.lastGroup != null)
					FPSInputModule.current.lastGroup.name.onScreen("input last group");

				$"{FPSInputModule.current.lockPauseMenu}".onScreen("lock pause menu");
			}

			$"{Cursor.lockState}".onScreen("cursor lock");
			if (false)
			{
				Button btn = uGUI.main.userInput.gameObject.getChild("Message/VerticalLayout/Button").GetComponent<Button>();

				//$"{btn.enabled} {btn.interactable} {btn.ish}".onScreen("button");
				$"{uGUI.main.userInput.gameObject.activeSelf}".onScreen("gameobject");
				$"{uGUI.main.userInput.isActiveAndEnabled}".onScreen("userinput");
				$"{Common.Debug.dumpGameObject(uGUI.main.userInput.gameObject, false, false)}".onScreen("userinput_dump");
			}
		}
	}


	class ConsoleCommands: PersistentConsoleCommands
	{
		public static void setLabel(string label)
		{
		}

		void OnConsoleCommand_makeprefabs(NotificationCenter.Notification _)
		{
			uGUI_PDA.main.gameObject.dump();
		}

		void OnConsoleCommand_testinput(NotificationCenter.Notification _)
		{
			//GameObject watcher = new GameObject();
			//watcher.AddComponent<GuiWatcher>();

			//uGUI.main.userInput.gameObject.dump();
			//uGUI.main.gameObject.dump();

			//uGUI_GraphicRaycaster cmp = uGUI.main.userInput.gameObject.ensureComponent<uGUI_GraphicRaycaster>();

			//cmp.guiCameraSpace = true;
			//cmp.copyFieldsFrom(IngameMenu.main.GetComponent<uGUI_GraphicRaycaster>());


			//__instance.gameObject.AddComponent<Canvas>();

			"!!!!!!!!!!!!!!!!!!!!!!".onScreen();
			uGUI.main.userInput.gameObject.dump("input_raycasternew");

			uGUI.main.userInput.RequestString(	Language.main.Get("BeaconLabel"),
												Language.main.Get("BeaconSubmit"),
												"OLOLO", 25, new uGUI_UserInput.UserInputCallback(setLabel));
		}

		void OnConsoleCommand_testmenu(NotificationCenter.Notification _)
		{
			IngameMenu.main.Open();
		}
	}
}