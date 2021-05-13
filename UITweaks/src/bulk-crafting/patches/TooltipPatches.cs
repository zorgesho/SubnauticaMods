using System;

using Harmony;

using Common;
using Common.Harmony;

#if GAME_SN
using UnityEngine;
using UnityEngine.UI;
#endif

namespace UITweaks
{
	static partial class BulkCraftingTooltip
	{
		[OptionalPatch, PatchClass]
		static class TooltipPatches
		{
			static bool prepare()
			{
				if (Main.config.bulkCrafting.enabled)
					init(uGUI_Tooltip.main); // in case we enable it after tooltip awake
#if GAME_SN
				else
					setActionText(AmountActionHint.None);
#endif
				return Main.config.bulkCrafting.enabled;
			}

			static CraftTree.Type currentTreeType;

			// prevents SMLHelper from restoring techdata to original state // TODO check for BZ
			//[HarmonyPrefix, HarmonyHelper.Patch("SMLHelper.V2.Patchers.CraftDataPatcher, SMLHelper", "NeedsPatchingCheckPrefix")]
			//static bool SMLPatchCheck(TechType techType) => currentTechType != techType || !CraftData.techData.ContainsKey(techType);

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_Tooltip), "Awake")]
			static void awakePatch(uGUI_Tooltip __instance) => init(__instance);
#if GAME_SN
			[HarmonyPrefix, HarmonyPatch(typeof(uGUI_Tooltip), "Set")]
			static void resetText() => setActionText(AmountActionHint.None);
#endif
			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_Tooltip), "OnUpdate")]
			static void checkVisible()
			{
				if (!uGUI_Tooltip.visible && currentTechType != TechType.None)
					reset();
			}

			[HarmonyPrefix, HarmonyPatch(typeof(uGUI_CraftingMenu), "Open")]
			static void openCraftingMenu(CraftTree.Type treeType, ITreeActionReceiver receiver)
			{
				currentTreeType = treeType;
				currentPowerRelay = Main.config.bulkCrafting.changePowerConsumption? (receiver as GhostCrafter)?.powerRelay: null;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(TooltipFactory), Mod.Consts.isGameSN? "Recipe": "CraftRecipe")]
#if GAME_SN
			static void updateRecipe(TechType techType)
#elif GAME_BZ
			static void updateRecipe(TechType techType, TooltipData data)
#endif
			{
				if (currentTreeType == CraftTree.Type.Constructor)
					return;

				if (techType != currentTechType)
					reset();

				if (currentTechType == TechType.None)
					init(techType);

				changeAmount(Math.Sign(InputHelper.getMouseWheelValue()));
#if GAME_SN
				updateActionHint();
#elif GAME_BZ
				string action = getActionText();
				if (action != "")
					data.postfix.AppendLine(action);
#endif
			}
#if GAME_SN
			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_Tooltip), "Rebuild")]
			static void rebuildTooltip(uGUI_Tooltip __instance, CanvasUpdate executing) // TODO BRANCH_EXP: most of this code is unneeded on exp branch
			{
				const float tooltipOffsetX = 30f;

				if (text.text == "" || executing != CanvasUpdate.Layout)
					return;

				float tooltipHeight = -__instance.rectTransform.rect.y;
				float textHeight = text.rectTransform.sizeDelta.y;
				__instance.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tooltipHeight + textHeight);

				float tooltipWidth = __instance.rectTransform.rect.xMax;
				float textWidth = text.rectTransform.sizeDelta.x + tooltipOffsetX;
				if (tooltipWidth < textWidth)
					__instance.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

				float textPosY = __instance.iconCanvas.transform.localPosition.y -__instance.iconCanvas.rectTransform.sizeDelta.y;
				text.rectTransform.localPosition = new Vector2(textPosX, textPosY);
			}
#endif
		}
	}
}