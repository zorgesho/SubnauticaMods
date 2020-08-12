using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace UITweaks
{
	[OptionalPatch, PatchClass]
	static class CrafterPatches
	{
		static readonly Dictionary<CrafterLogic, CraftData.TechData> crafterCache = new Dictionary<CrafterLogic, CraftData.TechData>();

		static bool prepare() => Main.config.bulkCrafting;

		static bool _isAmountChanged(TechType techType) =>
			techType == BulkCraftingTooltip.currentTechType && BulkCraftingTooltip.currentCraftAmount > 1;

		[HarmonyPrefix, HarmonyPatch(typeof(Crafter), "Craft")]
		static void craftFixDuration(TechType techType, ref float duration)
		{
			if (_isAmountChanged(techType))
				duration *= BulkCraftingTooltip.currentCraftAmount;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(CrafterLogic), "Craft")]
		static void craftUpdateCache(CrafterLogic __instance, TechType techType)
		{
			if (_isAmountChanged(techType))
				crafterCache[__instance] = BulkCraftingTooltip.currentTechData;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), "Craft")]
		static void craftFixAmount(CrafterLogic __instance, TechType techType)
		{
			if (_isAmountChanged(techType) && BulkCraftingTooltip.originalTechData.craftAmount == 0)
				__instance.numCrafted = 0;
		}

		[HarmonyTranspiler, HarmonyPatch(typeof(GhostCrafter), "Craft")]
		static IEnumerable<CodeInstruction> craftFixEnergyConsumption(IEnumerable<CodeInstruction> cins)
		{
			static float _energyToConsume(TechType techType) =>
				_isAmountChanged(techType)? 5f * BulkCraftingTooltip.currentCraftAmount: 5f;

			return CIHelper.ciReplace(cins, ci => ci.isLDC(5f), OpCodes.Ldarg_1, CIHelper.emitCall<Func<TechType, float>>(_energyToConsume));
		}

		[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), "Reset")]
		static void reset(CrafterLogic __instance) => crafterCache.Remove(__instance);

		[HarmonyTranspiler]
		[HarmonyHelper.Patch(typeof(CrafterLogic), Mod.isBranchStable? "TryPickup": "TryPickupAsync")]
#if BRANCH_EXP
		[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.PatchIteratorMethod)]
#endif
		static IEnumerable<CodeInstruction> pickup(IEnumerable<CodeInstruction> cins)
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