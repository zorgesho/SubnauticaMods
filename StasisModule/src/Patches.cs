using HarmonyLib;
using UnityEngine;

using Common.Stasis;
using Common.Harmony;

namespace StasisModule
{
	[PatchClass]
	static class Vehicle_OnUpgradeModuleUse_Patch
	{
		static bool isStasisModule(TechType techType)
		{
			return techType == PrawnSuitStasisModule.TechType ||
#if GAME_SN
			techType == SeaMothStasisModule.TechType;
#elif GAME_BZ
			techType == SeaTruckStasisModule.TechType;
#endif
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleUse")]
#if GAME_SN
		[HarmonyPatch(typeof(SeaMoth), "OnUpgradeModuleUse")]
#elif GAME_BZ
		[HarmonyPatch(typeof(SeaTruckUpgrades), "OnUpgradeModuleUse")]
#endif
		static void Vehicle_OnUpgradeModuleUse_Postfix(MonoBehaviour __instance, TechType techType, int slotID)
		{
			if (!isStasisModule(techType))
				return;

			// didn't find a better way :(
			bool slotUsed = __instance switch
			{
				Vehicle v => useStasisModuleSlot(v, slotID),
#if GAME_BZ
				SeaTruckUpgrades s => useStasisModuleSlot(s, slotID),
#endif
				_ => false
			};

			if (slotUsed)
				StasisSphereCreator.create(__instance.transform.position, Main.config.stasisTime, Main.config.stasisRadius);
		}

		static bool useStasisModuleSlot(Vehicle vehicle, int slotID)
		{
			if (!vehicle.ConsumeEnergy(Main.config.energyCost))
				return false;

			vehicle.quickSlotTimeUsed[slotID] = Time.time;
			vehicle.quickSlotCooldown[slotID] = Main.config.cooldown;
			return true;
		}
#if GAME_BZ
		static bool useStasisModuleSlot(SeaTruckUpgrades seatruck, int slotID)
		{
			if (!seatruck.ConsumeEnergy(Main.config.energyCost))
				return false;

			seatruck.quickSlotTimeUsed[slotID] = Time.time;
			seatruck.quickSlotCooldown[slotID] = Main.config.cooldown;
			return true;
		}

		// for some reason, seatruck starts cooldown for the slot even if there is not enough energy
		[HarmonyPrefix, HarmonyPatch(typeof(SeaTruckUpgrades), "OnUpgradeModuleUse")]
		static bool SeaTruckUpgrades_OnUpgradeModuleUse_Prefix(SeaTruckUpgrades __instance, TechType techType) =>
			techType != SeaTruckStasisModule.TechType || __instance.relay.GetPower() > Main.config.energyCost;
#endif
	}
}