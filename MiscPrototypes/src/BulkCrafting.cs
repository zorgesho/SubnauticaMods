using System.Collections.Generic;
using System.Text;

using Harmony;

using Common;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(TooltipFactory), "Recipe")]
	static class TooltipFactory_Recipe_Patch
	{
		static void Prefix(TechType techType)
		{
			"RECIPTE".onScreen("REC");
			CraftData.techData.TryGetValue(techType, out CraftData.TechData techData);

			if (InputHelper.getMouseWheelValue() > 0)
				techData._craftAmount += 1;
			else if (InputHelper.getMouseWheelValue() < 0)
				techData._craftAmount -= 1;

			//techData._craftAmount = getCraftAmount(scrapCount, smallScrapCount);
			//CraftData.craftingTimes[TechType.Titanium] = 0.7f * techData._craftAmount;
//			extraPowerConsumption = getPowerConsumption(techData._craftAmount);

			//techData._ingredients.Clear();
			//if (scrapCount > 0)
			//	techData._ingredients.Add(TechType.ScrapMetal, scrapCount);
			//if (smallScrapCount > 0)
			//	techData._ingredients.Add(ScrapMetalSmall.TechType, smallScrapCount);
		}

		
		static bool ___Prefix(TechType techType, bool locked, out string tooltipText, List<TooltipIcon> tooltipIcons)
		{
			TooltipFactory.Initialize();
			StringBuilder stringBuilder = new StringBuilder();
			if (locked)
			{
				TooltipFactory.WriteTitle(stringBuilder, Language.main.Get(techType));
				TooltipFactory.WriteDescription(stringBuilder, TooltipFactory.stringLockedRecipeHint);
				tooltipText = stringBuilder.ToString();
				return false;
			}
			ITechData techData = CraftData.Get(techType, false);
			string text = Language.main.Get(techType);
			int num = (techData == null) ? 1 : techData.craftAmount;
			if (num > 1)
			{
				text = Language.main.GetFormat<string, int>("CraftMultipleFormat", text, num);
			}
			TooltipFactory.WriteTitle(stringBuilder, text);
			TooltipFactory.WriteDescription(stringBuilder, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));
			if (techData != null)
			{
				TooltipFactory.WriteIngredients(techData, tooltipIcons);
			}

				
			tooltipText = stringBuilder.ToString();
			return false;
		}
	}
}