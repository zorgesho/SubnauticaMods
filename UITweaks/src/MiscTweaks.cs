using System.Collections;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

#if GAME_SN
	using Text = UnityEngine.UI.Text;
#elif GAME_BZ
	using Text = TMPro.TextMeshProUGUI;
#endif

#if GAME_BZ
using System.Text;
#endif

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
			const string textPath = (Mod.Consts.isGameSNExp? "SaveDetails/": "") + "SaveGameLength";

			static bool Prepare() => Main.config.showSaveSlotID;

			static void Postfix(MainMenuLoadButton lb)
			{
				var textGO = lb.load.getChild(textPath);

				if (!textGO)
				{
					"MainMenuLoadPanel_UpdateLoadButtonState_Patch: text not found".logError();
					return;
				}
#if GAME_BZ
				var rt = textGO.transform as RectTransform;
				RectTransformExtensions.SetSize(rt, 190f, rt.rect.height);
#endif
				if (textGO.TryGetComponent<Text>(out var text))
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

#if GAME_BZ
		[OptionalPatch, PatchClass]
		static class MetalDetectorTargetSwitcher
		{
			static bool prepare() => Main.config.switchMetalDetectorTarget;

			static readonly string buttons = Strings.Mouse.scrollUp + "/" + Strings.Mouse.scrollDown;

			static void changeTarget(MetalDetector md, int dir)
			{
				if (dir != 0)
					md.targetTechTypeIndex = MathUtils.mod(md.targetTechTypeIndex + dir, md.detectableTechTypes.Count);
			}

			static string getCurrentTarget(MetalDetector md)
			{
				bool indexValid = MathUtils.isInRange(md.targetTechTypeIndex, md.detectableTechTypes.Count - 1);
				return !indexValid? "": Language.main.Get(md.detectableTechTypes[md.targetTechTypeIndex].AsString());
			}

			[HarmonyPostfix, HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
			static void TooltipFactory_ItemCommons_Postfix(StringBuilder sb, TechType techType, GameObject obj)
			{
				if (techType != TechType.MetalDetector)
					return;

				if (obj.GetComponent<MetalDetector>() is MetalDetector md && md.energyMixin?.charge > 0)
				{
					changeTarget(md, InputHelper.getMouseWheelDir());
					TooltipFactory.WriteDescription(sb, L10n.str("ids_metalDetectorTarget") + getCurrentTarget(md));
				}
			}

			[HarmonyPostfix, HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
			static void TooltipFactory_ItemActions_Postfix(StringBuilder sb, InventoryItem item)
			{
				if (item.item.GetTechType() == TechType.MetalDetector && item.item.GetComponent<MetalDetector>()?.energyMixin?.charge > 0)
					TooltipFactory.WriteAction(sb, buttons, L10n.str("ids_metalDetectorSwitchTarget"));
			}
		}
#endif // GAME_BZ
	}
}