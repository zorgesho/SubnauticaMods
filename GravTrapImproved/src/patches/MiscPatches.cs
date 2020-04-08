using Harmony;

namespace GravTrapImproved
{
	[HarmonyPatch(typeof(SeaTreaderSounds), "SpawnChunks")]
	public static class SeaTreaderSounds_SpawnChunks_Patch
	{
		static bool Prepare() => Main.config.treaderSpawnChunkProbability != 1f;

		static bool Prefix() => UnityEngine.Random.value <= Main.config.treaderSpawnChunkProbability;
	}
}