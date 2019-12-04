using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine.Events;

using Common;

namespace MiscPatches
{
	using Instructions = IEnumerable<CodeInstruction>;

	// turn off vehicle lights by default
	[HarmonyPatch(typeof(Vehicle), "Start")]
	static class Vehicle_Start_Patch_LightsOffByDefault
	{
		const float time = 0.1f;

		static void Postfix(Vehicle __instance) =>
			__instance.gameObject.callAfterDelay(time, new UnityAction(() =>
			{
				if (__instance.gameObject.GetComponentInChildren<ToggleLights>() is ToggleLights lights)
				{
					// turn light off. Not using SetLightsActive because of sound
					lights.lightsActive = false;
					lights.lightsParent.SetActive(false);
				}
			}));
	}


	// Hide extra quick slots in vehicles
	// Modules installed in these slots working as usual
	// Intended for passive modules, issues with selectable modules
	static class VehiclesLessQuickSlots
	{
		static int _seamoth(Vehicle vehicle) => Math.Min(vehicle.slotIDs.Length, Main.config.maxSlotsCountSeamoth);
		static int _prawn(Vehicle vehicle)   => Math.Min(vehicle.slotIDs.Length, Main.config.maxSlotsCountPrawnSuit + 2);

		static Instructions ci(Instructions cins, bool isSeamoth)
		{
			var list = new List<CodeInstruction>(cins);

			list.RemoveRange(0, 5);

			list.InsertRange(0, new List<CodeInstruction>()
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Call, typeof(VehiclesLessQuickSlots).method(isSeamoth? "_seamoth": "_prawn")),
				new CodeInstruction(OpCodes.Stloc_0)
			});

			return list.AsEnumerable();
		}

		[HarmonyPatch(typeof(Vehicle), "GetSlotBinding", new Type[] {})]
		static class Vehicle_GetSlotBinding_Patch
		{
			static Instructions Transpiler(Instructions cins) => ci(cins, true);
		}

		[HarmonyPatch(typeof(Exosuit), "GetSlotBinding", new Type[] {})]
		static class Exosuit_GetSlotBinding_Patch
		{
			static Instructions Transpiler(Instructions cins) => ci(cins, false);
		}
	}


	// get access to seamoth torpedo tubes when docked in moonpool
	[HarmonyPatch(typeof(SeaMoth), "OnDockedChanged")]
	static class SeaMoth_OnDockedChanged_Patch
	{
		static void Postfix(SeaMoth __instance, Vehicle.DockType dockType)
		{
			foreach (var silo in new string[] {"TorpedoSiloLeft", "TorpedoSiloRight"})
				__instance.transform.Find(silo)?.gameObject.SetActive(dockType != Vehicle.DockType.Cyclops);
		}
	}


	// Fix hatch and antennas for docked vehicles in cyclops
	// Playing vehicle dock animation after load, dont find another way
	// Exosuit is also slightly moved from cyclops dock bay hatch, need to play all docking animations to fix it (like in moonpool)
	[HarmonyPatch(typeof(Vehicle), "Start")]
	static class Vehicle_Start_Patch_CyclopsDocking
	{
		const float time = 7f;
			
		static void Postfix(Vehicle __instance)
		{
			if (!__instance.docked)
				return;

			SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
			if (subRoot != null && !subRoot.isBase) // we're docked in cyclops
			{
				(__instance as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", true);

				__instance.gameObject.callAfterDelay(time, new UnityAction(() =>
				{
					(__instance as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", false);
				}));
			}
		}
	}
}