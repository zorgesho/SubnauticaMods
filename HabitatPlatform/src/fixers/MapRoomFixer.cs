using HarmonyLib;
using UnityEngine;

using Common;

namespace HabitatPlatform
{
	// component for fixing hologram map inside map room
	// updates shader property and reloads the map with delay
	class MapRoomFixer: MonoBehaviour
	{
		const int frameDelay = 300;

		int framePosChanged = 0;
		Vector3 lastPosition;

		MapRoomFunctionality mrf;

		void Awake()
		{																									$"MapRoomFixer: added".logDbg();
			lastPosition = transform.position;
			mrf = GetComponent<MapRoomFunctionality>();
		}

		void LateUpdate()
		{
			if (framePosChanged > 0 && Time.frameCount > framePosChanged + frameDelay)
			{																								$"MapRoomFixer: reloading map".logDbg();
				framePosChanged = 0;
				mrf.ReloadMapWorld();
			}

			if (transform.position == lastPosition)
				return;
																											$"MapRoomFixer: map room position changed ({lastPosition} => {transform.position})".logDbg();
			framePosChanged = Time.frameCount;
			lastPosition = transform.position;
			mrf.matInstance.SetVector(ShaderPropertyID._MapCenterWorldPos, lastPosition);
		}

		[HarmonyPatch(typeof(MapRoomFunctionality), "Start")]
		static class MapRoomFunctionality_Start_Patch
		{
			static void Postfix(MapRoomFunctionality __instance)
			{
				if (__instance.GetComponentInParent<HabitatPlatform.Tag>())
					__instance.gameObject.AddComponent<MapRoomFixer>();
			}
		}
	}
}