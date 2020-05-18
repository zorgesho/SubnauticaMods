using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace SeamothStorageSlots
{
	static class SeamothStorageInputPatches
	{
		static ItemsContainer getStorageInSlot(Vehicle vehicle, int slotID, TechType techType) =>
			 vehicle.GetStorageInSlot(slotID, techType) ?? vehicle.GetStorageInSlot(slotID + Main.config.slotsOffset, techType);

		// substitute call for 'this.seamoth.GetStorageInSlot()' with method above
		static IEnumerable<CodeInstruction> substSlotGetter(IEnumerable<CodeInstruction> cins)
		{
			MethodInfo substMethod = typeof(SeamothStorageInputPatches).method(nameof(getStorageInSlot));

			return CIHelper.ciReplace(cins, ci => ci.isOp(OpCodes.Callvirt), OpCodes.Call, substMethod);
		}

		[HarmonyPatch(typeof(SeamothStorageInput), "OpenPDA")]
		static class OpenPDA_Patch
		{
			static bool Prepare() => Main.config.slotsOffset > 0;
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) => substSlotGetter(cins);
		}

		[HarmonyPatch(typeof(SeamothStorageInput), "OnHandClick")]
		static class OnHandClick_Patch
		{
			static bool Prepare() => Main.config.slotsOffset > 0;
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) => substSlotGetter(cins);
		}
	}


	[HarmonyPatch(typeof(Equipment), "AllowedToAdd")]
	static class Equipment_AllowedToAdd_Patch
	{
		static bool Prepare() => Main.config.slotsOffset > 0;

		[HarmonyPriority(Priority.HigherThanNormal)]
		static bool Prefix(Equipment __instance, string slot, Pickupable pickupable, bool verbose, ref bool __result)
		{
			TechType techType = pickupable.GetTechType();

			if (techType != TechType.VehicleStorageModule || !slot.StartsWith("SeamothModule"))
				return true;

			SeaMoth seamoth = __instance.owner.GetComponent<SeaMoth>();
			if (seamoth == null)
				return true;

			int slotID = int.Parse(slot.Substring(13)) - 1;

			if (slotID > 3 && (slotID < Main.config.slotsOffset || slotID > Main.config.slotsOffset + 3))
				return true;

			// HACK: trying to swap one storage to another while drag, silently refusing because of ui problems
			if (seamoth.GetSlotItem(slotID)?.item.GetTechType() == TechType.VehicleStorageModule)
			{
				__result = false;
				return false;
			}

			__result = !seamoth.storageInputs[slotID % Main.config.slotsOffset].state; //already active

			if (!__result && verbose)
				$"Storage module is already in slot {(slotID < 4? slotID + Main.config.slotsOffset: slotID - Main.config.slotsOffset) + 1}".onScreen();

			return false;
		}
	}


	[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleChange")]
	static class Vehicle_OnUpgradeModuleChange_Patch
	{
		static bool Prepare() => Main.config.slotsOffset > 0;

		static void Postfix(Vehicle __instance, int slotID, TechType techType, bool added)
		{
			if (__instance is SeaMoth seamoth)
			{
				//any non-storage module added in seamoth slots 1-4 disables corresponding storage, checking if we need to enable it again
				if (slotID < 4 && techType != TechType.VehicleStorageModule)
				{
					if (__instance.GetSlotItem(slotID + Main.config.slotsOffset)?.item.GetTechType() == TechType.VehicleStorageModule)
						seamoth.storageInputs[slotID].SetEnabled(true);
				}
				else // if we adding/removing storage module in linked slots, we need to activate/deactivate corresponing storage unit
				if (slotID >= Main.config.slotsOffset && slotID < Main.config.slotsOffset + 4 && techType == TechType.VehicleStorageModule)
				{
					seamoth.storageInputs[slotID - Main.config.slotsOffset].SetEnabled(added);
				}
			}
		}
	}
}