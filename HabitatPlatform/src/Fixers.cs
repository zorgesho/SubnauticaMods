using UnityEngine;
using Common;

namespace HabitatPlatform
{
	// hacky way to fix collision bugs
	static class PlatformCollisionFixer
	{
		public static void fix(GameObject platform)
		{
			if (!platform)
				return;
																												$"PlatformFixer: fixing collisions".logDbg();
			Common.Debug.assert(platform.GetComponent<HabitatPlatform.Tag>());

			var rb = platform.GetComponent<Rigidbody>();
			rb.position = rb.position.setY(Main.config.defPosY);
		}
	}

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