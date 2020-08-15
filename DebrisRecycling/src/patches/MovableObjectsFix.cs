using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;

namespace DebrisRecycling
{
	[PatchClass]
	static class MovableObjectsFix
	{
		static void trySwitchCellLevel(GameObject go)
		{
			var lwe = go.GetComponent<LargeWorldEntity>();

			if	(lwe && (lwe.cellLevel == LargeWorldEntity.CellLevel.Medium || lwe.cellLevel == LargeWorldEntity.CellLevel.Far) &&
				 go.GetComponent<Rigidbody>()?.isKinematic == true &&
				!go.GetComponent<ImmuneToPropulsioncannon>())
			{																												$"Cell level switched for {go.name}".logDbg();
				lwe.cellLevel = LargeWorldEntity.CellLevel.Near;
			}
		}

		[HarmonyPrefix, HarmonyPatch(typeof(PropulsionCannon), "GrabObject")]
		static void PropulsionCannon_GrabObject_Prefix(GameObject target)
		{
			if (Main.config.fixLandscapeCollisions)
				trySwitchCellLevel(target);

			target.GetComponent<ResourceTracker>()?.StartUpdatePosition();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(RepulsionCannon), "ShootObject")]
		static void RepulsionCannon_ShootObject_Prefix(Rigidbody rb)
		{
			if (Main.config.fixLandscapeCollisions)
				trySwitchCellLevel(rb.gameObject);

			rb.gameObject.GetComponent<ResourceTracker>()?.StartUpdatePosition();
		}
	}
}