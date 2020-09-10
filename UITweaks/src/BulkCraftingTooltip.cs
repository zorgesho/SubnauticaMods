using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace UITweaks
{
	using CIEnumerable = IEnumerable<CodeInstruction>;

	static class BulkCraftingTooltip
	{
		static Text text;
		static float textPosX;

		static TechType currentTechType;
		static CraftData.TechData currentTechData, originalTechData;
		static int currentCraftAmount, currentCraftAmountMax;

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

		class BulkCraftingInitedTag: MonoBehaviour {}

		static void init(uGUI_Tooltip tooltip)
		{
			if (!tooltip || tooltip.GetComponent<BulkCraftingInitedTag>())
				return;

			tooltip.gameObject.AddComponent<BulkCraftingInitedTag>();

			var textGO = tooltip.gameObject.getChild(Mod.isBranchStable? "Text": "Container/Text");
			var textGOBottom = UnityEngine.Object.Instantiate(textGO, textGO.transform.parent);
			textGOBottom.name = "BottomText";

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

		static int getCountAvailable(TechType techType)
		{
			return EasyCraft_GetPickupCount? EasyCraft_GetPickupCount.invoke(techType): Inventory.main.GetPickupCount(techType);
		}

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
				currentPowerRelay = Main.config.bulkCrafting.changePowerConsumption? (receiver as GhostCrafter)?.powerRelay: null;
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


		[OptionalPatch, PatchClass]
		static class CrafterPatches
		{
			static bool prepare() => Main.config.bulkCrafting.enabled;

			static readonly Dictionary<CrafterLogic, CraftData.TechData> crafterCache = new Dictionary<CrafterLogic, CraftData.TechData>();

			static bool _isAmountChanged(TechType techType) =>
				techType == currentTechType && currentCraftAmount > 1;

			[OptionalPatch, PatchClass]
			static class CraftDurationPatches
			{
				static bool prepare() => CrafterPatches.prepare() && Main.config.bulkCrafting.changeCraftDuration;

				static void fixCraftDuration(TechType techType, ref float duration)
				{
					if (_isAmountChanged(techType))
						duration *= currentCraftAmount;
				}

				[HarmonyPriority(Priority.HigherThanNormal)] // just in case
				[HarmonyPrefix, HarmonyHelper.Patch(typeof(Crafter), "Craft")]
				static void fixCraftDuration_Vanilla(TechType techType, ref float duration) => fixCraftDuration(techType, ref duration);

				// compatibility patches for EasyCraft mod for fixing craft duration
				[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.CanBeAbsent)]
				[HarmonyPrefix, HarmonyHelper.Patch("EasyCraft.GhostCrafter_Craft_Patch, EasyCraft", "Prefix")]
				static void fixCraftDuration_EasyCraft(TechType techType, ref float duration) => fixCraftDuration(techType, ref duration);

				// EasyCraft clamps duration to 20 sec, we set it to 5 min
				[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.CanBeAbsent)]
				[HarmonyTranspiler, HarmonyHelper.Patch("EasyCraft.Main, EasyCraft", "GhostCraft")]
				static CIEnumerable extendDurationClamp(CIEnumerable cins)
				{
					var list = cins.ToList();
					int i = list.ciFindIndexForLast(ci => ci.isLDC(20f));

					if (i != -1)
						list[i].operand = 300f;

					return cins;
				}
			}

			[HarmonyPrefix, HarmonyPatch(typeof(CrafterLogic), "Craft")]
			static void craftUpdateCache(CrafterLogic __instance, TechType techType)
			{
				if (_isAmountChanged(techType))
					crafterCache[__instance] = currentTechData;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), "Craft")]
			static void craftFixAmount(CrafterLogic __instance, TechType techType)
			{
				if (_isAmountChanged(techType) && originalTechData.craftAmount == 0)
					__instance.numCrafted = 0;
			}

			[HarmonyTranspiler, HarmonyPatch(typeof(GhostCrafter), "Craft")]
			static CIEnumerable craftFixEnergyConsumption(CIEnumerable cins)
			{
				static float _energyToConsume(TechType techType) =>
					(Main.config.bulkCrafting.changePowerConsumption && _isAmountChanged(techType))? 5f * currentCraftAmount: 5f;

				return CIHelper.ciReplace(cins, ci => ci.isLDC(5f), OpCodes.Ldarg_1, CIHelper.emitCall<Func<TechType, float>>(_energyToConsume));
			}

			[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), "Reset")]
			static void reset(CrafterLogic __instance) => crafterCache.Remove(__instance);

			[HarmonyTranspiler]
			[HarmonyHelper.Patch(typeof(CrafterLogic), Mod.isBranchStable? "TryPickup": "TryPickupAsync")]
#if BRANCH_EXP
			[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.PatchIteratorMethod)]
#endif
			static CIEnumerable pickup(CIEnumerable cins)
			{
				var list = cins.ToList();

				var get_linkedItemCount = typeof(ITechData).method("get_linkedItemCount");
				int index = list.ciFindIndexForLast(ci => ci.isOp(OpCodes.Callvirt, get_linkedItemCount),
													ci => ci.isOp(OpCodes.Ldc_I4_1));

				return index == -1? cins:
					list.ciInsert(index + 2,
						Mod.isBranchStable? OpCodes.Ldarg_0: OpCodes.Ldloc_1,
						CIHelper.emitCall<Action<CrafterLogic>>(_changeLinkedItemsAmount));

				static void _changeLinkedItemsAmount(CrafterLogic instance)
				{
					if (crafterCache.TryGetValue(instance, out CraftData.TechData data))
						instance.numCrafted = data.craftAmount;
				}
			}
		}
	}
}