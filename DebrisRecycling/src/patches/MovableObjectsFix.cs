using Harmony;
using UnityEngine;

using Common;

namespace DebrisRecycling
{
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

		[HarmonyPatch(typeof(PropulsionCannon), "GrabObject")]
		static class PropulsionCannon_GrabObject_Patch
		{
			static void Prefix(GameObject target)
			{
				if (Main.config.fixLandscapeCollisions)
					trySwitchCellLevel(target);

				target.GetComponent<ResourceTracker>()?.StartUpdatePosition();
			}
		}

		[HarmonyPatch(typeof(RepulsionCannon), "ShootObject")]
		static class RepulsionCannon_ShootObject_Patch
		{
			static void Prefix(Rigidbody rb)
			{
				if (Main.config.fixLandscapeCollisions)
					trySwitchCellLevel(rb.gameObject);

				rb.gameObject.GetComponent<ResourceTracker>()?.StartUpdatePosition();
			}
		}
	}
}