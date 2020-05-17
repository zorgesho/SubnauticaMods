using Harmony;
using UnityEngine;

using Common;

namespace FloatingCargoCrate
{
	[HarmonyPatch(typeof(StorageContainer), "OnHandHover")]
	static class StorageContainer_OnHandHover_Patch
	{
		static void Postfix(StorageContainer __instance)
		{
			var fccControl = __instance.GetComponentInParent<FloatingCargoCrateControl>();

			if (fccControl && fccControl.needShowBeaconText && HandReticle.main.interactText1 != "")
				HandReticle.main.interactText1 += L10n.str(L10n.ids_attachBeaconToCrate).format(uGUI.FormatButton(GameInput.Button.RightHand));
		}
	}

	[HarmonyPatch(typeof(Beacon), "OnPickedUp")]
	static class Beacon_OnPickedUp_Patch
	{
		static void Prefix(Beacon __instance) => FloatingCargoCrateControl.tryDetachBeacon(__instance);
	}

	[HarmonyPatch(typeof(Beacon), "Throw")]
	static class Beacon_Throw_Patch
	{
		static void Postfix(Beacon __instance)
		{
			foreach (var f in Object.FindObjectsOfType<FloatingCargoCrateControl>()) // maybe make it with collider triggers?
			{
				if (f.tryAttachBeacon(__instance))
				{
					L10n.str(L10n.ids_beaconAttached).onScreen();
					break;
				}
			}
		}
	}

	[HarmonyPatch(typeof(Builder), "CheckAsSubModule")]
	static class Builder_CheckAsSubModule_Patch
	{
		static bool Prefix(ref bool __result)
		{
			if (!Builder.prefab.GetComponent<FloatingCargoCrateControl>())
				return true;

			__result = false;

			if (Builder.placePosition.y > 0)
				return false;

			Transform aimTransform = Builder.GetAimTransform();
			Builder.placementTarget = null;

			if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
			{
				Builder.SetPlaceOnSurface(hit, ref Builder.placePosition, ref Builder.placeRotation);
				return false;
			}

			__result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, hit.collider);

			return false;
		}
	}
}