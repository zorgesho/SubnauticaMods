using Harmony;
using UnityEngine;

using Common;
using Common.Configuration;

namespace MiscPatches
{
	// for testing loot spawning (Main.config.loolSpawnRerollCount for control)
	[HarmonyHelper.OptionalPatch]
	[HarmonyPatch(typeof(CellManager), "GetPrefabForSlot")]
	static class CellManager_GetPrefabForSlot_Patch
	{
		static bool Prepare() => Main.config.dbg.loolSpawnRerollCount > 0;

		static bool Prefix(CellManager __instance, IEntitySlot slot, ref EntitySlot.Filler __result)
		{
			if (__instance.spawner == null || __instance.streamer.debugDisableSlotEnts)
			{
				__result = default;
				return false;
			}

			for (int i = 0; i < Main.config.dbg.loolSpawnRerollCount; i++)
			{
				__result = __instance.spawner.GetPrefabForSlot(slot, true);

				if (__result.count > 0 || slot.IsCreatureSlot())
					break;
			}

			return false;
		}
	}

	// ignore limits for propulsion cannon
	[HarmonyHelper.OptionalPatch]
	[HarmonyPatch(typeof(PropulsionCannon), "ValidateObject")]
	static class PropulsionCannon_ValidateObject_Patch
	{
		static bool Prepare() => Main.config.dbg.propulsionCannonIgnoreLimits;

		static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}

	// keep particles alive on pause
	[HarmonyPatch(typeof(VFXController), "SpawnFX")]
	static class VFXController_SpawnFX_Patch
	{
		static bool Prepare() => Main.config.dbg.keepParticleSystemsAlive;

		public class Purge: Config.Field.IAction
		{
			public void action() =>
				Object.FindObjectsOfType<VFXDestroyAfterSeconds>().forEach(vfx => vfx.lifeTime = 0f);
		}

		static void Postfix(VFXController __instance, int i)
		{
			if (__instance.emitters[i].instanceGO.GetComponent<VFXDestroyAfterSeconds>() is VFXDestroyAfterSeconds vfx)
				vfx.lifeTime = float.PositiveInfinity;
		}
	}
}