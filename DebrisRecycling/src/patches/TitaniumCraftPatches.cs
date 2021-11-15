using System;

using HarmonyLib;

using Common;
using Common.Harmony;

namespace DebrisRecycling
{
	[PatchClass]
	static class TitaniumCraft
	{
		static bool prepare() => Main.config.craftConfig.dynamicTitaniumRecipe;

		[HarmonyPrefix, HarmonyPatch(typeof(uGUI_CraftingMenu), "Open")]
		static void uGUICraftingMenu_Open_Prefix(CraftTree.Type treeType, ITreeActionReceiver receiver)
		{
			if (treeType == CraftTree.Type.Fabricator)
			{
				updateTitaniumRecipe();
				currentPowerRelay = (receiver as GhostCrafter)?.powerRelay;															$"Current power relay: '{currentPowerRelay}'".logDbg();
			}
		}

		[HarmonyPrefix, HarmonyPatch(typeof(TooltipFactory), "Recipe")]
		static void TooltipFactory_Recipe_Prefix(TechType techType, out string tooltipText)
		{
			tooltipText = null;

			if (techType != TechType.Titanium)
				return;

			if (Main.config.extraPowerConsumption && currentPowerRelay != null && currentPowerRelay.GetPower() < getPowerConsumption() + 7f) // +2 energy units in order not to drain energy completely
				resetTitaniumRecipe();
			else
				updateTitaniumRecipe();
		}

		[HarmonyPatch(typeof(GhostCrafter), "Craft")]
		static class GhostCrafter_Craft_Patch
		{
			static bool Prepare() => prepare() && Main.config.extraPowerConsumption;

			[HarmonyPriority(Priority.High)]
			static void Prefix(GhostCrafter __instance, TechType techType)
			{
				if (techType == TechType.Titanium && extraPowerConsumption > 0f)
					CrafterLogic.ConsumeEnergy(__instance.powerRelay, extraPowerConsumption);
			}
		}


		static (int scrapCount, int smallScrapCount) lastScrapCount; // scrap count in the inventory

		static float extraPowerConsumption = 0f;
		static PowerRelay currentPowerRelay = null;

		static (int scrapCount, int smallScrapCount) getScrapCount()
		{
			return (Inventory.main.GetPickupCount(TechType.ScrapMetal), Inventory.main.GetPickupCount(ScrapMetalSmall.TechType));
		}

		// how much titanium can we craft from scrap in the inventory
		static int getCraftAmount(int scrapCount = 0, int smallScrapCount = 0)
		{
			if (scrapCount == 0 && smallScrapCount == 0)
				(scrapCount, smallScrapCount) = getScrapCount();

			return scrapCount * Main.config.craftConfig.titaniumPerBigScrap + smallScrapCount * Main.config.craftConfig.titaniumPerSmallScrap;
		}

		// extra power consumption for crafting this amount of titanium
		static float getPowerConsumption(int craftAmount = 0)
		{
			if (!Main.config.extraPowerConsumption)
				return 0f;

			if (craftAmount == 0)
				craftAmount = getCraftAmount();

			return Math.Max(0f, 1.25f * craftAmount - 5f); // 5 is default power consumption, it is consumed anyway
		}

		static void setTitaniumRecipe(int scrapCount, int smallScrapCount)
		{																								$"Set titanium recipe: scrap:{scrapCount}, small scrap:{smallScrapCount}".logDbg();
			CraftData.techData.TryGetValue(TechType.Titanium, out CraftData.TechData techData);

			techData._craftAmount = getCraftAmount(scrapCount, smallScrapCount);
			CraftData.craftingTimes[TechType.Titanium] = 0.7f * techData._craftAmount;
			extraPowerConsumption = getPowerConsumption(techData._craftAmount);

			techData._ingredients.Clear();
			if (scrapCount > 0)
				techData._ingredients.Add(TechType.ScrapMetal, scrapCount);
			if (smallScrapCount > 0)
				techData._ingredients.Add(ScrapMetalSmall.TechType, smallScrapCount);
		}

		static void updateTitaniumRecipe()
		{
			var scrapCount = getScrapCount();

			if (lastScrapCount != scrapCount)
			{
				lastScrapCount = scrapCount;
				setTitaniumRecipe(lastScrapCount == default? 1: scrapCount.scrapCount, scrapCount.smallScrapCount);
			}
		}

		// reset recipe to default (if we have just small pieces of scrap in inventory, set recipe to "smallscrap => 1 titanium")
		static void resetTitaniumRecipe()
		{
			if (lastScrapCount == default)
				return;

			lastScrapCount = default;
			var (scrapCount, smallScrapCount) = getScrapCount();

			if (scrapCount == 0 && smallScrapCount > 0)
				setTitaniumRecipe(0, 1);
			else
				setTitaniumRecipe(1, 0);
		}
	}
}