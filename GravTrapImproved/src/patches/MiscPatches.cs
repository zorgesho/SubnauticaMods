using Harmony;

namespace GravTrapImproved
{
	[HarmonyPatch(typeof(SeaTreaderSounds), "SpawnChunks")]
	static class SeaTreaderSounds_SpawnChunks_Patch
	{
		static bool Prepare() => Main.config.treaderChunkSpawnFactor != 1f;

		static bool Prefix() => UnityEngine.Random.value <= Main.config.treaderChunkSpawnFactor;
	}

	[HarmonyPatch(typeof(Gravsphere), "Start")]
	static class Gravsphere_Start_Patch_CellLevel
	{
		static bool Prepare() => Main.config._changeTrapCellLevel != LargeWorldEntity.CellLevel.Near;

		static void Postfix(Gravsphere __instance) =>
			__instance.gameObject.GetComponent<LargeWorldEntity>().cellLevel = Main.config._changeTrapCellLevel;
	}
}