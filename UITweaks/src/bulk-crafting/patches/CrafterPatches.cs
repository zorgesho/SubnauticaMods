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
	using CIEnumerable = IEnumerable<CodeInstruction>;

	static partial class BulkCraftingTooltip
	{
		[OptionalPatch, PatchClass]
		static class CrafterPatches
		{
			static bool prepare() => Main.config.bulkCrafting.enabled;

			static readonly Dictionary<CrafterLogic, CraftData.TechData> crafterCache = new();

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
					crafterCache[__instance] = currentTechData;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), "Craft")]
			static void craftFixAmount(CrafterLogic __instance, TechType techType)
			{
				if (_isAmountChanged(techType) && originalTechData.craftAmount == 0)
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

			[HarmonyPostfix, HarmonyPatch(typeof(CrafterLogic), "Reset")]
			static void reset(CrafterLogic __instance) => crafterCache.Remove(__instance);

			[HarmonyTranspiler]
			[HarmonyHelper.Patch(typeof(CrafterLogic), Mod.Consts.isBranchStable? "TryPickup": "TryPickupAsync")]
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
						Mod.Consts.isBranchStable? OpCodes.Ldarg_0: OpCodes.Ldloc_1,
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