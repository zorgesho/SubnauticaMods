using System;
using Harmony;
using UnityEngine;
using Common;

namespace WarningsDisabler
{
	//"BasePowerUp": "HABITAT: Power restored. All primary systems online.",
	//"BasePowerDown": "HABITAT: Warning, emergency power only.",

 //   "OxygenWarning10": "Oxygen.",
 //   "OxygenWarning30": "Warning: 30 seconds of oxygen remaining.",
	//"DepthWarning100": "Warning: Passing 100 meters. Oxygen efficiency decreased.",
	//"DepthWarning200": "Warning: Passing 200 meters. Oxygen efficiency greatly decreased.",
 //   "VitalsOk": "Vital signs stabilizing.",
 //   "FoodLow": "Calorie intake recommended.",
 //   "FoodCritical": "Emergency, starvation imminent. Seek calorie intake immediately.",
 //   "FoodVeryLow": "Seek calorie intake.",
 //   "WaterLow": "Seek fluid intake.",
 //   "WaterCritical": "Seek fluid intake immediately.",
 //   "WaterVeryLow": "Seek fluid intake.",


//			"OvereatingWarning": "Warning: Without variety in your diet your fluid and nutrient levels will decrease.",


	[HarmonyPatch(typeof(VoiceNotification), "Play")]
	[HarmonyPatch(new Type[] { typeof(object[]) })]
	class VoiceNotification_Play_Patch
	{
		private static bool Prefix(VoiceNotification __instance, object[] args, bool __result)
		{
			__instance.text.onScreen().logDbg();
			return true;
		}
	}
	
	[HarmonyPatch(typeof(VoiceNotification), "Play")]
	[HarmonyPatch(new Type[] { })]
	class VoiceNotification_Play_Patch1
	{
		private static bool Prefix(VoiceNotification __instance)
		{
			__instance.text.onScreen().logDbg();
			return true;
		}
	}




	//class FoodWaterWarnings
	//{
	//	// remove hunger and thrist warnings
	//	[HarmonyPatch(typeof(Survival))]
	//	[HarmonyPatch("UpdateWarningSounds")]
	//	class Survival_UpdateWarningSounds_Patch
	//	{
	//		private static bool Prefix(Survival __instance, PDANotification[] soundList, float val, float prevVal, float threshold1, float threshold2)
	//		{
	//			return false;
	//		}
	//	}
	
	//	// remove vitals ok message
	//	[HarmonyPatch(typeof(PDANotification))]
	//	[HarmonyPatch("Play")]
	//	[HarmonyPatch(new Type[] { })]
	//	class PDANotification_Play_Patch
	//	{
	//		private static bool Prefix(PDANotification __instance)
	//		{
	//			return __instance.text != "VitalsOk";
	//		}
	//	}
	//}
}
