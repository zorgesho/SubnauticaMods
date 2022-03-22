using HarmonyLib;
using UnityEngine;

using Common;

namespace GravTrapImproved
{
#if GAME_SN
	// change treader chunks probability
	[HarmonyPatch(typeof(SeaTreaderSounds), "SpawnChunks")]
	static class SeaTreaderSounds_SpawnChunks_Patch
	{
		static bool Prepare() => Main.config.treaderChunkSpawnFactor != 1f;

		static bool Prefix() => Random.value <= Main.config.treaderChunkSpawnFactor;
	}
#endif
	// change grav trap cell level
	[HarmonyPatch(typeof(Gravsphere), "Start")]
	static class Gravsphere_Start_Patch_CellLevel
	{
		static bool Prepare() => Main.config._changeTrapCellLevel != LargeWorldEntity.CellLevel.Near;

		static void Postfix(Gravsphere __instance) =>
			__instance.gameObject.GetComponent<LargeWorldEntity>().cellLevel = Main.config._changeTrapCellLevel;
	}

	// hide grav trap rays
	[HarmonyPatch(typeof(Gravsphere), "AddAttractable")]
	static class Gravsphere_AddAttractable_Patch__Effects
	{
		static bool Prepare() => !Main.config.raysVisible;

		static void Postfix(Gravsphere __instance) => __instance.effects.ForEach(ray => ray.Value.enabled = false);
	}

	// sometimes gravtrap can lose creatures inside working range
	[HarmonyPatch(typeof(Gravsphere), "OnTriggerExit")]
	static class Gravsphere_OnTriggerExit_Patch
	{
		static bool Prefix(Gravsphere __instance, Collider collider)
		{
			float range = __instance.GetComponent<GravTrapMK2.Tag>()? Main.config.mk2Range: 17f;
			float distSqr = (__instance.transform.position - collider.transform.position).sqrMagnitude;									$"Gravsphere_OnTriggerExit_Patch: object: {collider.name} distance: {Mathf.Sqrt(distSqr)}".logDbg();

			return distSqr > range * range;
		}
	}
}