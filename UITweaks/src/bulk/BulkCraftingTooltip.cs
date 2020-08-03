using System;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Harmony;

namespace UITweaks
{
	static class BulkCraftingTooltip
	{
		static Text text;
		static float textPosX;

		public static TechType currentTechType { get; private set; }
		public static CraftData.TechData currentTechData { get; private set; }
		public static CraftData.TechData originalTechData { get; private set; }
		public static int currentCraftAmount { get; private set; }
		static int currentCraftAmountMax;

		static PowerRelay currentPowerRelay;

		static string _writeAction(string key) =>
			$"\n<size=20><color=#ffffffff>{key}</color> - <color=#00ffffff>{L10n.str(L10n.ids_changeAmount)}</color></size>";

		static readonly string[] actions =
		{
			"",
			_writeAction(Strings.Mouse.scrollUp + "/" + Strings.Mouse.scrollDown),
			_writeAction(Strings.Mouse.scrollUp),
			_writeAction(Strings.Mouse.scrollDown)
		};

		enum AmountActionHint { None = 0, Both = 1, Increase = 2, Decrease = 3 } // used as index for actions array

		static void init(uGUI_Tooltip tooltip)
		{
			var textGO = UnityEngine.Object.Instantiate(tooltip.gameObject.getChild("Text"), tooltip.transform);
			textGO.name = "BottomText";

			var sizeFitter = textGO.AddComponent<ContentSizeFitter>();
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

			text = textGO.GetComponent<Text>();
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.verticalOverflow = VerticalWrapMode.Truncate;
			textPosX = text.rectTransform.localPosition.x;

			text.text = _writeAction("tmp"); // adding temporary text to update rect size
		}


		static void init(TechType techType)
		{
			currentTechType = techType;
			var techData = CraftData.techData[techType];
			currentCraftAmountMax = getMaxAmount(techData);

			if (currentCraftAmountMax == 0)
			{
				currentCraftAmount = 0;
				return;
			}

			currentCraftAmount = 1;
			originalTechData = techData;
			currentTechData = makeCopy(techData);
			CraftData.techData[techType] = currentTechData;
		}


		static int getMaxAmount(CraftData.TechData techData)
		{
			int maxAmount = int.MaxValue;

			if (GameModeUtils.RequiresIngredients())
			{
				foreach (var ing in techData._ingredients)
					maxAmount = Math.Min(maxAmount, Inventory.main.GetPickupCount(ing.techType) / ing.amount);

				if (currentPowerRelay != null)
					maxAmount = Math.Min(maxAmount, (int)(currentPowerRelay.GetPower() / 5f - 1f));
			}

			return maxAmount;
		}


		static void reset()
		{
			if (originalTechData != null)
				CraftData.techData[currentTechType] = originalTechData;

			currentTechType = TechType.None;
			originalTechData = currentTechData = null;
		}


		static CraftData.TechData makeCopy(CraftData.TechData techData)
		{
			var copy = new CraftData.TechData()
			{
				_techType = techData._techType,
				_craftAmount = techData.craftAmount,
				_linkedItems = techData._linkedItems == null? null: new List<TechType>(techData._linkedItems),
				_ingredients = new CraftData.Ingredients()
			};

			techData._ingredients.ForEach(i => copy._ingredients.Add(i.techType, i.amount));

			return copy;
		}


		static void setActionText(AmountActionHint hintType)
		{
			if (text)
				text.text = actions[(int)hintType];
		}


		static void changeAmount(int delta)
		{
			if (delta == 0 || currentCraftAmount == 0)
				return;

			if ((currentCraftAmount == 1 && delta == -1) || (currentCraftAmount == currentCraftAmountMax && delta == 1))
				return;

			currentCraftAmount += delta;

			int originalCraftAmount = originalTechData.craftAmount == 0? 1: originalTechData.craftAmount; // in case we use only linked items
			currentTechData._craftAmount = originalCraftAmount * currentCraftAmount;

			for (int i = 0; i < currentTechData._ingredients.Count; i++)
				currentTechData._ingredients[i]._amount = originalTechData.GetIngredient(i).amount * currentCraftAmount;
		}


		static void updateActionHint()
		{
			if		(currentCraftAmountMax <= 1)				  setActionText(AmountActionHint.None);
			else if (currentCraftAmount == 1)					  setActionText(AmountActionHint.Increase);
			else if (currentCraftAmount == currentCraftAmountMax) setActionText(AmountActionHint.Decrease);
			else												  setActionText(AmountActionHint.Both);
		}


		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare()
			{
				setActionText(AmountActionHint.None); // in case we're going to unpatch
				return Main.config.bulkCrafting;
			}

			// prevents SMLHelper from restoring techdata to original state
			[HarmonyPrefix, HarmonyHelper.Patch("SMLHelper.V2.Patchers.CraftDataPatcher, SMLHelper", "NeedsPatchingCheckPrefix")]
			static bool SMLPatchCheck(TechType techType) => currentTechType != techType || !CraftData.techData.ContainsKey(techType);

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
			static void getPowerRelay(ITreeActionReceiver receiver)
			{
				currentPowerRelay = (receiver as GhostCrafter)?.powerRelay;
			}

			[HarmonyPrefix, HarmonyPatch(typeof(TooltipFactory), "Recipe")]
			static void updateRecipe(TechType techType)
			{
				if (techType != currentTechType)
					reset();

				if (currentTechType == TechType.None)
					init(techType);

				changeAmount(Math.Sign(InputHelper.getMouseWheelValue()));
				updateActionHint();
			}

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_Tooltip), "Rebuild")]
			static void rebuildTooltip(uGUI_Tooltip __instance, CanvasUpdate executing)
			{
				if (text.text == "" || executing != CanvasUpdate.Layout)
					return;

				float tooltipHeight = -__instance.rectTransform.rect.y;
				float textHeight = text.rectTransform.sizeDelta.y;
				__instance.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tooltipHeight + textHeight);

				float tooltipWidth = __instance.rectTransform.rect.xMax;
				float textWidth = text.rectTransform.sizeDelta.x + Main.config._tooltipOffsetX;
				if (tooltipWidth < textWidth)
					__instance.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

				float textPosY = __instance.iconCanvas.transform.localPosition.y -__instance.iconCanvas.rectTransform.sizeDelta.y;
				text.rectTransform.localPosition = new Vector2(textPosX, textPosY);
			}
		}
	}
}