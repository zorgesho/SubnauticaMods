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

			if (lwe?.cellLevel is LargeWorldEntity.CellLevel.Medium or LargeWorldEntity.CellLevel.Far &&
				go.GetComponent<Rigidbody>()?.isKinematic == true &&
				!go.GetComponent<ImmuneToPropulsioncannon>())
			{																											$"Cell level switched for {go.name}".logDbg();
				lwe.cellLevel = LargeWorldEntity.CellLevel.Near;
			}
		}

		static void fixObject(GameObject go)
		{
			if (Main.config.fixLandscapeCollisions)
				trySwitchCellLevel(go);

			go.GetComponent<ResourceTracker>()?.StartUpdatePosition();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(PropulsionCannon), "GrabObject")]
		static void PropulsionCannon_GrabObject_Prefix(GameObject target) => fixObject(target);

		[HarmonyPrefix, HarmonyPatch(typeof(RepulsionCannon), "ShootObject")]
		static void RepulsionCannon_ShootObject_Prefix(Rigidbody rb) => fixObject(rb.gameObject);
	}
}