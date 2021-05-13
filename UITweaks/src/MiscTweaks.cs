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

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_BuilderMenu), "GetToolbarTooltip")]
#if GAME_SN
			static void modifyTooltip(int index, ref string tooltipText)
#elif GAME_BZ
			static void modifyTooltip(int index, TooltipData data)
#endif
			{
				if (!Main.config.showToolbarHotkeys)
					return;

				string text = $"<size=25><color=#ADF8FFFF>{index + 1}</color> - </size>";
#if GAME_SN
				tooltipText = text + tooltipText;
#elif GAME_BZ
				data.prefix.Insert(0, text);
#endif
			}

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
			const string textPath = (Mod.Consts.isBranchStable? "": "SaveDetails/") + "SaveGameLength"; // TODO: fix for BZ

			static bool Prepare() => Main.config.showSaveSlotID;

			static void Postfix(MainMenuLoadButton lb)
			{
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