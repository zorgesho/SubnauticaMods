using System;
using Harmony;

// TODO: make with transpilers
namespace MiscPatches
{
	// Hide extra quick slots in vehicles
	// Modules installed in these slots working as usual
	// Intended for passive modules, issues with selectable modules
	[HarmonyPatch(typeof(Exosuit), "GetSlotBinding")]
	[HarmonyPatch(new Type[] {})]
	static class Exosuit_GetSlotBinding_Patch
	{
		static bool Prefix(Vehicle __instance, ref TechType[] __result)
		{
			int count = Math.Min(__instance.slotIDs.Length, Main.config.maxSlotsCountPrawnSuit + 2);
			__result = new TechType[count];

			for (int i = 0; i < count; i++)
			{
				TechType techType = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[i]);
				if (techType == TechType.None && (i == __instance.GetSlotIndex("ExosuitArmLeft") || i == __instance.GetSlotIndex("ExosuitArmRight")))
					techType = TechType.ExosuitClawArmModule;
				
				__result[i] = techType;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(Vehicle), "GetSlotBinding")]
	[HarmonyPatch(new Type[] {})]
	static class Vehicle_GetSlotBinding_Patch
	{
		static bool Prefix(Vehicle __instance, ref TechType[] __result)
		{
			int count = Math.Min(__instance.slotIDs.Length, Main.config.maxSlotsCountSeamoth);
			__result = new TechType[count];

			for (int i = 0; i < count; i++)
				__result[i] = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[i]);

			return false;
		}
	}
}