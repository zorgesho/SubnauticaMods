using System;
using System.Text;

using UnityEngine;
using Harmony;
using Common;

namespace FloatingCargoCrate
{
	[HarmonyPatch(typeof(StorageContainer))]
	[HarmonyPatch("OnHandHover")]
	public class StorageContainer_OnHandHover_Patch
	{
		static void Postfix(StorageContainer __instance, GUIHand hand)
		{
			FloatingCargoCrateControl fccControl = __instance.GetComponentInParent<FloatingCargoCrateControl>();
			
			if (fccControl && fccControl.needShowBeaconText && HandReticle.main.interactText1 != "")
				HandReticle.main.interactText1 += "\nAttach beacon to crate (" + uGUI.FormatButton(GameInput.Button.RightHand) + ")";
		}
	}

	[HarmonyPatch(typeof(Beacon), "OnPickedUp")]
	public class Beacon_OnPickedUp_Patch
	{
		static void Prefix(Beacon __instance, Pickupable p)
		{
			FloatingCargoCrateControl.tryDetachBeacon(__instance);
		}
	}


	[HarmonyPatch(typeof(Beacon), "Throw")]
	public class Beacon_Throw_Patch
	{
		static void Postfix(Beacon __instance)
		{
			FloatingCargoCrateControl[] fccControls = GameObject.FindObjectsOfType<FloatingCargoCrateControl>(); // maybe make it with collider triggers?

			foreach (var f in fccControls)
			{
				if (f.tryAttachBeacon(__instance))
				{
					ErrorMessage.AddDebug("Beacon attached");
					break;
				}
			}
		}
	}


	[HarmonyPatch(typeof(Builder), "CheckAsSubModule")]
	public class Builder_CheckAsSubModule_Patch
	{
		static bool Prefix(ref bool __result)
		{
			if (!Builder.prefab.GetComponent<FloatingCargoCrateControl>())
				return true;

			__result = false;
		
			Transform aimTransform = Builder.GetAimTransform();
			Builder.placementTarget = null;
			RaycastHit hit;

			if (Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
			{
				Builder.SetPlaceOnSurface(hit, ref Builder.placePosition, ref Builder.placeRotation);
				return false;
			}

			if (Builder.placePosition.y > 0)
				return false;

			__result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, hit.collider);

			return false;
		}
	}
}