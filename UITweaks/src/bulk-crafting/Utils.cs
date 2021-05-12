﻿using System.Linq;
using System.Collections.ObjectModel;

using Harmony;
using SMLHelper.V2.Handlers;

using Common.Harmony;
using Common.Crafting;

namespace UITweaks
{
	static partial class BulkCraftingTooltip
	{
		static class TechInfoUtils
		{
			public static TechInfo getTechInfo(TechType techType)
			{
#if GAME_SN
				return CraftDataHandler.GetTechData(techType);
#elif GAME_BZ
				return CraftDataHandler.GetRecipeData(techType);
#endif
			}

			public static void setTechInfo(TechType techType, TechInfo techInfo)
			{
#if GAME_SN
				CraftData.techData[techType] = techInfo;
#elif GAME_BZ
				// for BZ we using TechDataPatches below
#endif
			}
#if GAME_BZ
			[OptionalPatch, PatchClass]
			static class TechDataPatches
			{
				static bool prepare() => Main.config.bulkCrafting.enabled;

				[HarmonyPriority(Priority.High)]
				[HarmonyPrefix, HarmonyHelper.Patch(typeof(TechData), "GetCraftAmount")]
				static bool TechData_GetCraftAmount_Prefix(TechType techType, ref int __result)
				{
					if (techType != currentTechType)
						return true;

					__result = originalCraftAmount * currentCraftAmount;
					return false;
				}

				[HarmonyPriority(Priority.High)]
				[HarmonyPrefix, HarmonyHelper.Patch(typeof(TechData), "GetIngredients")]
				static bool TechData_GetIngredients_Prefix(TechType techType, ref ReadOnlyCollection<Ingredient> __result)
				{
					if (techType != currentTechType)
						return true;

					__result = currentTechInfo.ingredients.Select(ing => new Ingredient(ing.techType, ing.amount)).ToList().AsReadOnly();
					return false;
				}
			}
#endif
		}
	}
}