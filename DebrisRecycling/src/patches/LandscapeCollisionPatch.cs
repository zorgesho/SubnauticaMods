using Harmony;
using UnityEngine;

using Common;

namespace DebrisRecycling
{
	[HarmonyPatch(typeof(PropulsionCannon), "GrabObject")]
	static class PropulsionCannon_GrabObject_Patch
	{
		static void Prefix(GameObject target)
		{
			CellLevelSwitcher.tryAddTo(target);
			target.GetComponent<ResourceTracker>()?.StartUpdatePosition();
		}
	}

	[HarmonyPatch(typeof(RepulsionCannon), "ShootObject")]
	static class RepulsionCannon_ShootObject_Patch
	{
		static void Prefix(Rigidbody rb)
		{
			CellLevelSwitcher.tryAddTo(rb.gameObject);
			rb.gameObject.GetComponent<ResourceTracker>()?.StartUpdatePosition();
		}
	}


	class CellLevelSwitcher: MonoBehaviour
	{
		static bool isValidToAdd(GameObject go)
		{
			LargeWorldEntity lwe = go.GetComponent<LargeWorldEntity>();
			Rigidbody rigidbody = go.GetComponent<Rigidbody>();

			return  (lwe && rigidbody &&
					(lwe.cellLevel == LargeWorldEntity.CellLevel.Medium || lwe.cellLevel == LargeWorldEntity.CellLevel.Far) &&
					 rigidbody.isKinematic &&
					!go.GetComponent<ImmuneToPropulsioncannon>());
		}

		public static void tryAddTo(GameObject go)
		{
			if (isValidToAdd(go))
				go.AddComponent<CellLevelSwitcher>();
		}

		void OnCollisionEnter(Collision collision)
		{
			if (collision.collider.GetComponentInParent<Voxeland>())
			{																												$"Cell level switched for {gameObject.name}".logDbg();
				gameObject.GetComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
				Destroy(this); // our job is done
			}
		}
	}
}