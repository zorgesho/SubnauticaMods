using System;
using Harmony;
using UnityEngine;
using Common;

namespace WarningsDisabler
{
	//"CyclopsWelcomeAboard": "CYCLOPS: Welcome aboard captain. All systems online.",
	//"CyclopsWelcomeAboardAttention": "CYCLOPS: Welcome aboard captain. Some systems require attention.",
	
	//"SeamothWelcomeAboard" : "Seamoth: Welcome aboard captain.",
	//"SeamothWelcomeNoPower" : "Seamoth: Warning: Emergency power only. Oxygen production offline.",
	
	//"ExosuitWelcomeAboard" : "PRAWN: Welcome aboard captain.",
	//"ExosuitWelcomeNoPower" : "PRAWN: Warning: Emergency power only. Oxygen production offline.",
	
	//"BaseWelcomeAboard": "HABITAT: Welcome aboard captain.",
	//"BaseWelcomeNoPower": "HABITAT: Warning: Emergency power only. Oxygen production offline.",
	
	//"BasePowerUp": "HABITAT: Power restored. All primary systems online.",
	//"BasePowerDown": "HABITAT: Warning, emergency power only.",

	[HarmonyPatch(typeof(VoiceNotification), "Play", new Type[] { typeof(object[]) })]
	class VoiceNotification_Play_Patch
	{
		static bool Prefix(VoiceNotification __instance, object[] args, bool __result)
		{
			$"VoiceNotification {__instance.text}".onScreen().logDbg();
			return true;
		}
	}
	
	

	//"DepthWarning100": "Warning: Passing 100 meters. Oxygen efficiency decreased.",
	//"DepthWarning200": "Warning: Passing 200 meters. Oxygen efficiency greatly decreased.",
	
	//"VitalsOk": "Vital signs stabilizing.",
	//"FoodLow": "Calorie intake recommended.",
	//"FoodCritical": "Emergency, starvation imminent. Seek calorie intake immediately.",
	//"FoodVeryLow": "Seek calorie intake.",
	//"WaterLow": "Seek fluid intake.",
	//"WaterCritical": "Seek fluid intake immediately.",
	//"WaterVeryLow": "Seek fluid intake.",
	
	[HarmonyPatch(typeof(PDANotification), "Play", new Type[] { typeof(object[]) })]
	class PDANotification_Play_Patch
	{
		static bool Prefix(PDANotification __instance, object[] args)
		{
			$"PDANotification {__instance.text}".onScreen().logDbg();
			return true;
		}
	}
}
