using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Reflection;

namespace UITweaks
{
	static partial class BulkCraftingTooltip
	{
		static Text text;
		static float textPosX;

		static TechType currentTechType;
		static CraftData.TechData currentTechData, originalTechData;
		static int currentCraftAmount, currentCraftAmountMax;

		static PowerRelay currentPowerRelay;

		static string _writeAction(string key) =>
			$"\n<size=20><color=#ffffffff>{key}</color> - <color=#00ffffff>{L10n.str(L10n.ids_bulkCraftChangeAmount)}</color></size>";

		static readonly string[] actions =
		{
			"",
			_writeAction(Strings.Mouse.scrollUp),
			_writeAction(Strings.Mouse.scrollDown),
			_writeAction(Strings.Mouse.scrollUp + "/" + Strings.Mouse.scrollDown)
		};

		enum AmountActionHint { None = 0, Increase = 1, Decrease = 2, Both = 3 } // used as index for actions array

		class BulkCraftingInitedTag: MonoBehaviour {}

		static void init(uGUI_Tooltip tooltip)
		{
			if (!tooltip || tooltip.GetComponent<BulkCraftingInitedTag>())
				return;

			tooltip.gameObject.AddComponent<BulkCraftingInitedTag>();

			var textGO = tooltip.gameObject.getChild(Mod.Consts.isBranchStable? "Text": "Container/Text");
			var textGOBottom = textGO.getParent().createChild(textGO, "BottomText");

			var sizeFitter = textGOBottom.AddComponent<ContentSizeFitter>();
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

			text = textGOBottom.GetComponent<Text>();
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

		// if EasyCraft mod is installed we will use it to get count of available ingredients
		static readonly MethodWrapper<Func<TechType, int>> EasyCraft_GetPickupCount =
			Type.GetType("EasyCraft.ClosestItemContainers, EasyCraft")?.method("GetPickupCount").wrap<Func<TechType, int>>();

		static int getCountAvailable(TechType techType) =>
			EasyCraft_GetPickupCount?.invoke(techType) ?? Inventory.main.GetPickupCount(techType);

		static int getMaxAmount(CraftData.TechData techData)
		{
			int maxAmount = int.MaxValue;

			if (GameModeUtils.RequiresIngredients())
			{
				foreach (var ing in techData._ingredients)
					maxAmount = Math.Min(maxAmount, getCountAvailable(ing.techType) / ing.amount);

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
			CraftData.TechData copy = new()
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
			if (!text)
				return;

			text.text = actions[(int)hintType];
			text.gameObject.SetActive(hintType != AmountActionHint.None);
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
	}
}