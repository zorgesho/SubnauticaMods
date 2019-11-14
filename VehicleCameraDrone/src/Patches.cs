using UnityEngine;
using Harmony;

using Common;

namespace VehicleCameraDrone
{
	[HarmonyPatch(typeof(Player), "Update")]
	static class TooltipFactory_ItsdfemActionssdf
	{
		static void Postfix()
		{
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				MapRoomCamera mapRoomCamera = GameObject.FindObjectOfType<MapRoomCamera>();
				if (mapRoomCamera)
				{
					//mapRoomCamera.gameObject.AddComponent<Scanner>();
					mapRoomCamera.GetComponent<EnergyMixin>().allowBatteryReplacement = true;
					mapRoomCamera.ControlCamera(Player.main, null);

					ToggleLights tl = mapRoomCamera.gameObject.getOrAddComponent<ToggleLights>();
					tl.lightsParent = mapRoomCamera.gameObject.getChild("lights_parent");
					tl.energyMixin = mapRoomCamera.gameObject.GetComponent<EnergyMixin>();

					$"{tl.lightsParent} {tl.energyMixin}----------".log();

					Common.Debug.dumpGameObject(mapRoomCamera.gameObject, true, true).saveToFile("camera");
				}
			}
		}
	}

	[HarmonyPatch(typeof(MapRoomCamera), "Update")]
	static class TooltipFactory_ItsdfemActionssdsdff111
	{
		static void Postfix(MapRoomCamera __instance)
		{
			__instance.GetComponent<ToggleLights>()?.CheckLightToggle();
		}
	}

	[HarmonyPatch(typeof(MapRoomCamera), "FreeCamera")]
	static class TooltipFactory_ItsdfemActionssdf111
	{
		static bool Prefix(MapRoomCamera __instance, bool resetPlayerPosition)
		{
			InputHandlerStack.main.Pop(__instance.inputStackDummy);
			if (__instance.controllingPlayer.GetVehicle() == null)
				__instance.controllingPlayer.ExitLockedMode(false, false);	/////////////////////////////////////////////
			__instance.controllingPlayer = null;
			if (resetPlayerPosition)
			{
				SNCameraRoot.main.transform.localPosition = Vector3.zero;
				SNCameraRoot.main.transform.localRotation = Quaternion.identity;
			}
			__instance.rigidBody.velocity = Vector3.zero;
			MainCameraControl.main.enabled = true;
			__instance.screen?.OnCameraFree(__instance);		//////////////////////////
			__instance.screen = null;
			__instance.RenderToTexture();
			uGUI_CameraDrone.main.SetCamera(null);
			uGUI_CameraDrone.main.SetScreen(null);
			__instance.engineSound.Stop();
			__instance.screenEffectModel.SetActive(false);
			__instance.droneIdle.Stop();
			__instance.connectingSound.Stop();
			Player.main.SetHeadVisible(false);
			__instance.lightsParent.SetActive(false);

			return false;
		}
	}


}