using Harmony;
using UnityEngine;

using Common;

namespace GravTrapImproved
{
	[HarmonyPatch(typeof(Gravsphere), "Start")]
	static class Gravsphere_Start_Patch
	{
		static void Postfix(Gravsphere __instance) => __instance.gameObject.ensureComponent<GravTrapObjectsType>();
	}

	[HarmonyPatch(typeof(Gravsphere), "AddAttractable")]
	static class Gravsphere_AddAttractable_Patch
	{
		static void Postfix(Gravsphere __instance, Rigidbody r) =>
			__instance.GetComponent<GravTrapObjectsType>().handleAttracted(r.gameObject, true);
	}

	[HarmonyPatch(typeof(Gravsphere), "DestroyEffect")]
	static class Gravsphere_DestroyEffect_Patch
	{
		static void Postfix(Gravsphere __instance, int index)
		{
			var rigidBody = __instance.attractableList[index];
			if (rigidBody)
				__instance.GetComponent<GravTrapObjectsType>().handleAttracted(rigidBody.gameObject, false);
		}
	}

	[HarmonyPatch(typeof(Gravsphere), "IsValidTarget")]
	static class Gravsphere_IsValidTarget_Patch
	{
		static bool Prefix(Gravsphere __instance, GameObject obj, ref bool __result)
		{
			__result = __instance.GetComponent<GravTrapObjectsType>().isValidTarget(obj);
			return false;
		}
	}
}