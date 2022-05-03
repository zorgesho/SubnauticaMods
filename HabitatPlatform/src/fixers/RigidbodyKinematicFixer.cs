using UnityEngine;

namespace HabitatPlatform
{
	partial class PlatformInitializer
	{
		// component that enables physics right after construction, so platform can fall into the water
		// component disables physics during the construction and for already constructed platforms
		class RigidbodyKinematicFixer: MonoBehaviour
		{
			Rigidbody rb;

			float timeBuildCompleted;
			const float delayForDisablingPhysics = 5f;

			public static Rigidbody disablePhysics(GameObject go)
			{
				var rb = go.GetComponent<Rigidbody>();
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
				rb.isKinematic = true;

				return rb;
			}

			void Awake()
			{
				rb = disablePhysics(gameObject);
			}

			// enabling physics right after construction
			void SubConstructionComplete()
			{
				rb.isKinematic = false;
				timeBuildCompleted = Time.time;
			}

			void FixedUpdate()
			{
				if (!rb.isKinematic && timeBuildCompleted + delayForDisablingPhysics < Time.time && rb.velocity.sqrMagnitude < 0.1f)
				{
					rb.isKinematic = true;
					Destroy(this);
				}
			}
		}
	}
}