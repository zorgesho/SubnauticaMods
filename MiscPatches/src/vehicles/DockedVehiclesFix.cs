using Harmony;
using UnityEngine;

namespace MiscPatches
{
	// get access to seamoth torpedo tubes when docked in moonpool
	[HarmonyPatch(typeof(SeaMoth), "OnDockedChanged")]
	static class SeaMoth_OnDockedChanged_Patch
	{
		static void Postfix(SeaMoth __instance, Vehicle.DockType dockType)
		{
			Transform[] torpedoSilos = {__instance.transform.Find("TorpedoSiloLeft"), __instance.transform.Find("TorpedoSiloRight") };

			foreach (var torpedoSilo in torpedoSilos)
				torpedoSilo.gameObject.SetActive(dockType != Vehicle.DockType.Cyclops);
		}
	}

	// Fix hatch and antennas for docked vehicles in cyclops
	// Playing vehicle dock animation after load, dont find another way
	// Exosuit is also slightly moved from cyclops dock bay hatch, need to play all docking animations to fix it (like in moonpool)
	class CyclopsDockAnimationFix
	{
		class CallAfterDelay: MonoBehaviour
		{
			const float t = 7f;
			void Start() => Invoke(nameof(playAnimation), t);

			void playAnimation()
			{
				(GetComponent<Vehicle>() as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", false);
				Destroy(this);
			}
		}

		[HarmonyPatch(typeof(Vehicle), "Start")]
		static class Vehicle_Start_Patch
		{
			static void Postfix(Vehicle __instance)
			{
				if (!__instance.docked)
					return;

				SubRoot subRoot = __instance.GetComponentInParent<SubRoot>();
				if (subRoot != null && !subRoot.isBase) // we're docked in cyclops
				{
					(__instance as IAnimParamReceiver).ForwardAnimationParameterBool("cyclops_dock", true);
					__instance.gameObject.AddComponent<CallAfterDelay>();
				}
			}
		}
	}
}