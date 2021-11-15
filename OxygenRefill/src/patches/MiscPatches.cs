using HarmonyLib;
using UnityEngine;

using Common;

namespace OxygenRefill
{
	// add oxygen only to player
	[HarmonyPatch(typeof(Oxygen), "AddOxygen")]
	static class Oxygen_AddOxygen_Patch
	{
		static bool Prefix(Oxygen __instance) => __instance.isPlayer;
	}

	// setting correct oxygen tank capacity after loading
	[HarmonyPatch(typeof(Oxygen), "Awake")]
	static class Oxygen_Awake_Patch
	{
		static void Prefix(Oxygen __instance)
		{
			float capacity = Main.config.getTankCapacity(__instance.gameObject);

			if (capacity > 0)
				__instance.oxygenCapacity = capacity;
		}
	}

	// prevents registering oxygen source when picking up items
	[HarmonyPatch(typeof(Inventory), "TryUpdateOxygen")]
	static class Inventory_TryUpdateOxygen_Patch
	{
		static bool Prefix() => false;
	}

	// consume energy for oxygen production while player in the vehicle
	// don't replentish oxygen for player here, because it replentishes elsewhere anyway (CanBreathe)
	[HarmonyPatch(typeof(Vehicle), "ReplenishOxygen")]
	static class Vehicle_ReplenishOxygen_Patch
	{
		static bool Prefix(Vehicle __instance)
		{
			if (__instance.turnedOn && __instance.replenishesOxygen && __instance.GetPilotingMode() && __instance.CanPilot())
			{
				__instance.energyInterface.ConsumeEnergy(Time.deltaTime * __instance.oxygenEnergyCost);
#if DEBUG
				__instance.energyInterface.GetValues(out float charge, out float capacity);
				$"{charge}".onScreen("vehicle energy");
#endif
			}

			return false;
		}
	}
}