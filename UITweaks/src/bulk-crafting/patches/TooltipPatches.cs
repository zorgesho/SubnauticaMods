using System;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Harmony;

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
				else
					setActionText(AmountActionHint.None);

				return Main.config.bulkCrafting.enabled;
			}

			static CraftTree.Type currentTreeType;

			// prevents SMLHelper from restoring techdata to original state // TODO check for BZ
			//[HarmonyPrefix, HarmonyHelper.Patch("SMLHelper.V2.Patchers.CraftDataPatcher, SMLHelper", "NeedsPatchingCheckPrefix")]
			//static bool SMLPatchCheck(TechType techType) => currentTechType != techType || !CraftData.techData.ContainsKey(techType);

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_Tooltip), "Awake")]
			static void awakePatch(uGUI_Tooltip __instance) => init(__instance);

			[HarmonyPrefix, HarmonyPatch(typeof(uGUI_Tooltip), "Set")]
			static void resetText() => setActionText(AmountActionHint.None);

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

			[HarmonyPrefix, HarmonyPatch(typeof(TooltipFactory), Mod.Consts.isGameSN? "Recipe": "CraftRecipe")]
			static void updateRecipe(TechType techType)
			{
				if (currentTreeType == CraftTree.Type.Constructor)
					return;

				if (techType != currentTechType)
					reset();

				if (currentTechType == TechType.None)
					init(techType);

				changeAmount(Math.Sign(InputHelper.getMouseWheelValue()));
				//updateActionHint(); // TODO
			}

			//[HarmonyPostfix, HarmonyPatch(typeof(uGUI_Tooltip), "Rebuild")] // TODO
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
		}
	}
}