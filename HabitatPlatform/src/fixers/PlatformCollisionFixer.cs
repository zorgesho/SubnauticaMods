using UnityEngine;
using Common;

namespace HabitatPlatform
{
	// hacky way to fix collision bugs
	static class PlatformCollisionFixer
	{
		class PosFixer: MonoBehaviour
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
}