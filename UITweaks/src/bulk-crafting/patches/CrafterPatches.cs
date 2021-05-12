using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Harmony;
using Common.Crafting;
using Common.Reflection;

namespace UITweaks
{
	static partial class BulkCraftingTooltip
	{
		[OptionalPatch, PatchClass]
		static class CrafterPatches
		{
			static bool prepare() => Main.config.bulkCrafting.enabled;

			static readonly Dictionary<CrafterLogic, TechInfo> crafterCache = new();

			static bool _isAmountChanged(TechType techType) =>
				techType == currentTechType && currentCraftAmount > 1;

			[HarmonyPriority(Priority.HigherThanNormal)] // just in case
			[HarmonyPrefix, HarmonyHelper.Patch(typeof(Crafter), "Craft")]
			static void craftFixDuration(TechType techType, ref float duration)
			{
				if (Main.config.bulkCrafting.changeCraftDuration && _isAmountChanged(techType))
					duration *= currentCraftAmount;
			}

			[HarmonyPrefix, HarmonyPatch(typeof(CrafterLogic), "Craft")]
			static void craftUpdateCache(CrafterLogic __instance, TechType techType)
			{
				if (_isAmountChanged(techType))
					crafterCache[__instance] = currentTechInfo;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), "Craft")]
			static void craftFixAmount(CrafterLogic __instance, TechType techType)
			{
				if (_isAmountChanged(techType) && originalTechInfo.craftAmount == 0)
					__instance.numCrafted = 0;
			}

			[HarmonyPriority(Priority.HigherThanNormal)]
			[HarmonyPrefix, HarmonyPatch(typeof(GhostCrafter), "Craft")]
			static void craftFixEnergyConsumption(GhostCrafter __instance, TechType techType)
			{
				if (!Main.config.bulkCrafting.changePowerConsumption)
					return;

				if (_isAmountChanged(techType))
					CrafterLogic.ConsumeEnergy(__instance.powerRelay, (currentCraftAmount - 1) * 5f); // and 5f also consumed in the vanilla method
			}

			[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), Mod.Consts.isGameSN? "Reset": "ResetCrafter")]
			static void reset(CrafterLogic __instance) => crafterCache.Remove(__instance);

			[HarmonyTranspiler]
			[HarmonyHelper.Patch(typeof(CrafterLogic), Mod.Consts.isGameSNStable? "TryPickup": "TryPickupAsync")]
#if !(GAME_SN && BRANCH_STABLE)
			[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.PatchIteratorMethod)]
#endif
			static IEnumerable<CodeInstruction> fixLinkedItemCount(IEnumerable<CodeInstruction> cins)
			{
				var list = cins.ToList();

				var numCrafted = typeof(CrafterLogic).field("numCrafted");
				int index = list.ciFindIndexForLast(ci => ci.isOp(OpCodes.Stfld, numCrafted),
													ci => ci.isOp(OpCodes.Stfld, numCrafted));
				return index == -1? cins:
					list.ciReplace(index - 1,
						Mod.Consts.isGameSNStable? OpCodes.Ldarg_0: OpCodes.Ldloc_1,
						CIHelper.emitCall<Func<CrafterLogic, int>>(_getNumCrafted));

				static int _getNumCrafted(CrafterLogic instance) =>
					crafterCache.TryGetValue(instance, out TechInfo info)? info.craftAmount: 1;
			}
		}
	}
}