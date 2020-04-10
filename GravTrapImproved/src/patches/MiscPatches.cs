using Harmony;

namespace GravTrapImproved
{
	// change treader chunks probability
	[HarmonyPatch(typeof(SeaTreaderSounds), "SpawnChunks")]
	static class SeaTreaderSounds_SpawnChunks_Patch
	{
		static bool Prepare() => Main.config.treaderChunkSpawnFactor != 1f;

		static bool Prefix() => UnityEngine.Random.value <= Main.config.treaderChunkSpawnFactor;
	}

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
}