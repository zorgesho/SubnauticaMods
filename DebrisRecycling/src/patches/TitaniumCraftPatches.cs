using System;
using Harmony;
using Common;

namespace DebrisRecycling
{
	static class TitaniumCraft
	{
		[HarmonyPatch(typeof(uGUI_CraftingMenu), "Open")]
		static class uGUICraftingMenu_Open_Patch
		{
			static bool Prepare() => Main.config.craftConfig.dynamicTitaniumRecipe;

			static void Prefix(CraftTree.Type treeType, ITreeActionReceiver receiver)
			{
				if (treeType == CraftTree.Type.Fabricator)
				{
					updateTitaniumRecipe();
					currentPowerRelay = (receiver as GhostCrafter)?.powerRelay;															$"Current power relay: '{currentPowerRelay}'".logDbg();
				}
			}
		}

		[HarmonyPatch(typeof(TooltipFactory), "Recipe")]
		static class TooltipFactory_Recipe_Patch
		{
			static bool Prepare() => Main.config.craftConfig.dynamicTitaniumRecipe;

			static void Prefix(TechType techType, out string tooltipText)
			{
				tooltipText = null;

				if (techType != TechType.Titanium)
					return;

				if (Main.config.extraPowerConsumption && currentPowerRelay != null && currentPowerRelay.GetPower() < getPowerConsumption() + 7f) // +2 energy units in order not to drain energy completely
					resetTitaniumRecipe();
				else
					updateTitaniumRecipe();
			}
		}

		[HarmonyPatch(typeof(GhostCrafter), "Craft")]
		static class GhostCrafter_Craft_Patch
		{
			static bool Prepare() => Main.config.craftConfig.dynamicTitaniumRecipe && Main.config.extraPowerConsumption;

			[HarmonyPriority(Priority.High)]
			static void Prefix(GhostCrafter __instance, TechType techType)
			{
				if (techType == TechType.Titanium && extraPowerConsumption > 0f)
					CrafterLogic.ConsumeEnergy(__instance.powerRelay, extraPowerConsumption);
			}
		}


		static int lastScrapCount = 0; // scrapCount * 100 + smallScrapCount;

		static float extraPowerConsumption = 0f;
		static PowerRelay currentPowerRelay = null;

		static void getScrapCount(out int scrapCount, out int smallScrapCount)
		{
			scrapCount = Inventory.main.GetPickupCount(TechType.ScrapMetal);
			smallScrapCount = Inventory.main.GetPickupCount(ScrapMetalSmall.TechType);
		}

		// how much titanium can we craft from scrap in inventory
		static int getCraftAmount(int scrapCount = 0, int smallScrapCount = 0)
		{
			if (scrapCount == 0 && smallScrapCount == 0)
				getScrapCount(out scrapCount, out smallScrapCount);

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
			getScrapCount(out int scrapCount, out int smallScrapCount);

			int sum = scrapCount * 100 + smallScrapCount;
			if (lastScrapCount != sum)
			{
				lastScrapCount = sum;
				setTitaniumRecipe((lastScrapCount == 0? 1: scrapCount), smallScrapCount);
			}
		}

		// reset recipe to default (if we have just small pieces of scrap in inventory, set recipe to "smallscrap => 1 titanium")
		static void resetTitaniumRecipe()
		{
			if (lastScrapCount == 0)
				return;

			lastScrapCount = 0;
			getScrapCount(out int scrapCount, out int smallScrapCount);

			if (scrapCount == 0 && smallScrapCount > 0)
				setTitaniumRecipe(0, 1);
			else
				setTitaniumRecipe(1, 0);
		}
	}
}