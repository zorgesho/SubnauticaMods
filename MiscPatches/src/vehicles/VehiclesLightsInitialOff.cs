using Harmony;
using UnityEngine;

namespace MiscPatches
{//TODO make one component LightsOff and set variable togglelight at start
	class VehiclesLightsPatches
	{
		class LightsOffSeamoth: MonoBehaviour
		{
			const float t = 0.1f;
			void Start() { Invoke("lightsOff", t); }
		
			void lightsOff()
			{
				ToggleLights lights = GetComponent<SeaMoth>().toggleLights;
				
				// turn light off. Not using SetLightsActive because of sound
				lights.lightsActive = false;
				lights.lightsParent.SetActive(false);

				Destroy(this);
			}
		}
		

		[HarmonyPatch(typeof(SeaMoth), "Start")]
		static class SeaMoth_Start_Patch
		{
			static void Postfix(SeaMoth __instance) => __instance.gameObject.AddComponent<LightsOffSeamoth>();
		}


		class LightsOffExosuit: MonoBehaviour
		{
			const float t = 0.1f;
			void Start() => Invoke("lightsOff", t);

			void lightsOff()
			{
				ToggleLights lights = GetComponent<ToggleLights>();

				if (lights)  // works if PrawnSuitLightSwitch mod is installed
				{
					// turn light off. Not using SetLightsActive because of sound
					lights.lightsActive = false;
					lights.lightsParent.SetActive(false);
				}

				Destroy(this);
			}
		}


		[HarmonyPatch(typeof(Exosuit), "Start")]
		static class Exosuit_Start_Patch
		{
			static void Postfix(Exosuit __instance) => __instance.gameObject.AddComponent<LightsOffExosuit>();
		}
	}
}