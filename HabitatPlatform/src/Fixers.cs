using System.Collections;
using UnityEngine;
using Common;

namespace HabitatPlatform
{
	// hacky way to fix collision bugs
	static class PlatformCollisionFixer
	{
		class PosFixer: MonoBehaviour // fixers are never enough
		{
			Vector3 initPos;

			void Awake() => initPos = gameObject.transform.position;

			void LateUpdate()
			{																									$"PlatformCollisionFixer.PosFixer: fixing {gameObject.transform.position.ToString("F4")} -> {initPos.ToString("F4")}".logDbg();
				gameObject.transform.position = initPos;
				Destroy(this);
			}
		}

		public static void fix(GameObject platform)
		{
			if (!platform)
				return;
																												$"PlatformFixer: fixing collisions".logDbg();
			Common.Debug.assert(platform.GetComponent<HabitatPlatform.Tag>());

			var rb = platform.GetComponent<Rigidbody>();
			rb.position = rb.position.setY(Main.config.defPosY);

			platform.AddComponent<PosFixer>();
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

		// component for fixing transform after loading
		// reason for this bug is unknown for now
		class TransformFixer: MonoBehaviour
		{
			protected Transform tr;

			protected Vector3 initPos;
			protected Quaternion initRot;

			protected virtual void Awake()
			{
				tr = gameObject.transform;

				initPos = tr.position;
				initRot = tr.rotation;																			$"TransformFixer ({tr.name}):  init pos: {initPos.ToString("F4")} rot: {initRot.ToString("F4")}".logDbg();
			}

			IEnumerator Start()
			{
				while (GameUtils.isLoadingState)
				{
					if (initPos != tr.position || initRot != tr.rotation)
					{																							$"TransformFixer ({tr.name}): fixing transform - pos: {tr.position.ToString("F4")} rot: {tr.rotation.ToString("F4")}".logDbg();
						fix();
					}

					yield return null;
				}

				Destroy(this);
			}

			protected virtual void fix() => tr.SetPositionAndRotation(initPos, initRot);
		}

		// component for fixing player transform after loading
		// for Player it's not enough to just use Transform component
		class PlayerTransformFixer: TransformFixer
		{
			Player player;

			protected override void Awake()
			{
				base.Awake();
				player = gameObject.GetComponent<Player>();

				Common.Debug.assert(player != null);

				if (GameUtils.getVehicle(player))
				{																								$"PlayerTransformFixer: player is in the vehicle, aborting".logDbg();
					Destroy(this);
				}
			}

			protected override void fix() => player?.SetPosition(initPos, initRot);
		}
	}
}