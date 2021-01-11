using System.Collections;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Harmony;

namespace UITweaks
{
	static class MiscTweaks
	{
		[OptionalPatch, PatchClass]
		static class BuilderMenuHotkeys
		{
			static bool prepare() => Main.config.builderMenuTabHotkeysEnabled;

#if GAME_SN // TODO: fix for BZ
			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_BuilderMenu), "GetToolbarTooltip")]
			static void modifyTooltip(int index, ref string tooltipText)
			{
				if (Main.config.showToolbarHotkeys)
					tooltipText = $"<size=25><color=#ADF8FFFF>{index + 1}</color> - </size>{tooltipText}";
			}
#endif
			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_BuilderMenu), "Open")]
			static void openMenu()
			{
				UWE.CoroutineHost.StartCoroutine(_builderMenuTabHotkeys());

				static IEnumerator _builderMenuTabHotkeys()
				{
					while (uGUI_BuilderMenu.singleton.state)
					{
						for (int i = 0; i < 5; i++)
							if (Input.GetKeyDown(KeyCode.Alpha1 + i))
								uGUI_BuilderMenu.singleton.SetCurrentTab(i);

						yield return null;
					}
				}
			}
		}

		// add game slot info to the load buttons
		[OptionalPatch, HarmonyPatch(typeof(MainMenuLoadPanel), "UpdateLoadButtonState")]
		public static class MainMenuLoadPanel_UpdateLoadButtonState_Patch
		{
			static bool Prepare() => Main.config.showSaveSlotID;

			static void Postfix(MainMenuLoadButton lb)
			{
				string textPath = (Mod.Consts.isBranchStable? "": "SaveDetails/") + "SaveGameLength"; // TODO: fix for BZ
				if (lb.load.getChild(textPath)?.GetComponent<Text>() is Text text)
					text.text += $" | {lb.saveGame}";
			}
		}

		// don't show messages while loading
		[OptionalPatch, HarmonyPatch(typeof(ErrorMessage), "AddError")]
		static class ErrorMessage_AddError_Patch
		{
			static bool Prepare() => Main.config.hideMessagesWhileLoading;
			static bool Prefix() => !GameUtils.isLoadingState;
		}
	}
}