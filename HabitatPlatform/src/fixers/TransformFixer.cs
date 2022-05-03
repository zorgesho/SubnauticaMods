using System.Collections;
using UnityEngine;
using Common;

namespace HabitatPlatform
{
	partial class PlatformInitializer
	{
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