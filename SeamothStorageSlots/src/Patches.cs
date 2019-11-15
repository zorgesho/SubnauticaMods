#define SLOTS9_12

using Harmony;
using Common;

namespace SeamothStorageSlots
{
	[HarmonyPatch(typeof(SeamothStorageInput), "OnHandClick")]
	static class SeamothStorageInput_OnHandClick_Patch
	{
		static bool Prefix(SeamothStorageInput __instance)
		{
			// dont check anything, just open the box
			__instance.ChangeFlapState(true, true);
			return false;
		}
	}

		
	[HarmonyPatch(typeof(SeamothStorageInput), "OpenPDA")]
	static class SeamothStorageInput_OpenPDA_Patch
	{
		static bool Prefix(SeamothStorageInput __instance)
		{
#if SLOTS9_12
			ItemsContainer storageInSlot = __instance.seamoth.GetStorageInSlot(__instance.slotID + 8, TechType.VehicleStorageModule);
#else
			ItemsContainer storageInSlot = __instance.seamoth.GetStorageInSlot(__instance.slotID, TechType.VehicleStorageModule);
				
			if (storageInSlot == null)
				storageInSlot = __instance.seamoth.GetStorageInSlot(__instance.slotID + 4, TechType.VehicleStorageModule);
#endif
			if (storageInSlot != null)
			{
				PDA pda = Player.main.GetPDA();
				Inventory.main.SetUsedStorage(storageInSlot, false);
				if (!pda.Open(PDATab.Inventory, __instance.tr, new PDA.OnClose(__instance.OnClosePDA), -1f))
					__instance.OnClosePDA(pda);
			}
			else
			{
				__instance.OnClosePDA(null);
			}

			return false;
		}
	}


	[HarmonyPatch(typeof(Equipment), "AllowedToAdd")]
	static class Equipment_AllowedToAdd_Patch
	{
		static bool Prefix(Equipment __instance, string slot, Pickupable pickupable, bool verbose, ref bool __result)
		{
			TechType techType = pickupable.GetTechType();	
			__result = true;

			// no storage modules allowed in "linked" slots (slot N and slot N+4)
			if (techType == TechType.VehicleStorageModule && slot.StartsWith("SeamothModule"))
			{
				int slotID = int.Parse(slot.Substring(13));
				
				if (slotID < 9)
				{
#if SLOTS9_12
					__result = false;
				}
				
				return false;
#else
					SeaMoth seamoth = __instance.owner.GetComponent<SeaMoth>();
				
					if (seamoth != null)
					{
						InventoryItem itemInSlot = seamoth.GetSlotItem(slotID - 1);
					
						// HACK: trying to swap one storage to another while drag, silently refusing because of ui problems
						if (itemInSlot != null && seamoth.GetSlotItem(slotID - 1).item.GetTechType() == TechType.VehicleStorageModule)
						{
							__result = false;
							return false;
						}
			
						SeamothStorageInput seamothStorageInput = seamoth.storageInputs[((slotID < 5)? slotID - 1: slotID - 5)];

						if (seamothStorageInput.state) //already active
						{
							$"Storage module is already in slot {((slotID < 5)? slotID + 4: slotID - 4)}".onScreen();
							__result = false;
						}
						
						return false;
					}
				}
#endif
			}

			return true;
		}
	}


	[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleChange")]
	static class Vehicle_OnUpgradeModuleChange_Patch
	{
		static void Postfix(Vehicle __instance, int slotID, TechType techType, bool added)
		{
			if (__instance.GetType() == typeof(SeaMoth))
			{
				//any non-storage module added in seamoth slots 1-4 disables corresponding storage, checking if we need to enable it again
				if (slotID < 4 && techType != TechType.VehicleStorageModule)
				{
#if SLOTS9_12
					if (__instance.GetSlotItem(slotID + 8) != null &&__instance.GetSlotItem(slotID + 8).item.GetTechType() == TechType.VehicleStorageModule)
#else
					if (__instance.GetSlotItem(slotID + 4) != null &&__instance.GetSlotItem(slotID + 4).item.GetTechType() == TechType.VehicleStorageModule)
#endif
						(__instance as SeaMoth).storageInputs[slotID].SetEnabled(true);
				}
				// if we adding/removing storage module in slots 4-8, we need to activate/deactivate corresponing storage unit
#if SLOTS9_12
				else if (slotID >= 8 && slotID <= 12 && techType == TechType.VehicleStorageModule) 
					((__instance as SeaMoth).storageInputs[slotID - 8]).SetEnabled(added);
#else
				else if (slotID > 3 && slotID < 8 && techType == TechType.VehicleStorageModule) 
					((__instance as SeaMoth).storageInputs[slotID - 4]).SetEnabled(added);
#endif
			}
		}
	}
}