using HarmonyLib;
using UnityEngine;

using Common;

namespace HabitatPlatform
{
	class MapRoomCameraTransformFix: MonoBehaviour
	{
		MapRoomCamera camera;
		void init(MapRoomCamera camera) => this.camera = camera;

		void Awake() => "FixCameraTransform: added".logDbg();
		void OnDestroy() => "FixCameraTransform: removed".logDbg();

		void Update()
		{
			if (!camera?.dockingPoint || !camera.rigidBody.isKinematic)
			{
				Destroy(this);
				return;
			}

			camera.transform.rotation = camera.dockingPoint.dockingTransform.rotation;
			camera.transform.position = camera.dockingPoint.dockingTransform.position;
		}

		[HarmonyPatch(typeof(MapRoomCamera), "SetDocked")]
		static class MapRoomCamera_SetDocked_Patch
		{
			static void Postfix(MapRoomCamera __instance, MapRoomCameraDocking dockingPoint)
			{
				if (dockingPoint?.GetComponentInParent<HabitatPlatform.Tag>())
					__instance.gameObject.ensureComponent<MapRoomCameraTransformFix>().init(__instance);
			}
		}
	}
}