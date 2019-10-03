using System;
using Harmony;
using UnityEngine;
using Common;

namespace WarningsDisabler
{
	//"BasePowerUp": "HABITAT: Power restored. All primary systems online.",
	//"BasePowerDown": "HABITAT: Warning, emergency power only.",


	// remove messages about habitat power right after load
	class HabitatWarnings
	{
		public class WarningsDisabled: MonoBehaviour
		{
			static public bool isWarningsDisabled = false;
			
			public float t = 1f;
			public void Start()
			{
				isWarningsDisabled = true; // first created instance disables warnings
				Invoke("Stop", t);
			}
		
			public void Stop()
			{
				isWarningsDisabled = false; // first destroyed instance enables warnings back
				UnityEngine.Object.Destroy(this);
			}
		}

		[HarmonyPatch(typeof(VoiceNotification))]
		[HarmonyPatch("Play")]
		[HarmonyPatch(new Type[] { typeof(object[]) })]
		class VoiceNotification_Play_Patch
		{
			private static bool Prefix(VoiceNotification __instance, object[] args, bool __result)
			{
				__result = WarningsDisabled.isWarningsDisabled;
				return !WarningsDisabled.isWarningsDisabled;
			}
		}

		[HarmonyPatch(typeof(PowerRelay))]
		[HarmonyPatch("Start")]
		class PowerRelay_Start_Patch
		{
			private static void Postfix(PowerRelay __instance)
			{
				__instance.gameObject.AddComponent<WarningsDisabled>();
			}
		}
	
		[HarmonyPatch(typeof(BasePowerRelay))]
		[HarmonyPatch("PowerDownEvent")]
		class BasePowerRelay_PowerDownEvent_Patch
		{
			private static bool Prefix(BasePowerRelay __instance, PowerRelay relay)
			{
				return !WarningsDisabled.isWarningsDisabled;
			}
		}


		[HarmonyPatch(typeof(BasePowerRelay))]
		[HarmonyPatch("PowerUpEvent")]
		class BasePowerRelay_PowerUpEvent_Patch
		{
			private static bool Prefix(BasePowerRelay __instance, PowerRelay relay)
			{
				return !WarningsDisabled.isWarningsDisabled;
			}
		}
	}
}
