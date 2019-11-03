using Common;
using Common.Configuration;

namespace GravTrapImproved
{
	public static class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			//Options.init();
#if VER_1_2_0
			checkOptionalPatches();
#endif
		}

#if VER_1_2_0
		internal static void checkOptionalPatches()
		{
			if (config.maxRadius != 17f)
				HarmonyHelper.setPatchEnabled(true, typeof(OptionalPatches.Gravsphere_MaxRadius_Patch));

			if (config.maxObjects != 12)
				HarmonyHelper.setPatchEnabled(true, typeof(OptionalPatches.Gravsphere_MaxObjects_Patch));

			if (config.maxForce != 15f)
				HarmonyHelper.setPatchEnabled(true, typeof(OptionalPatches.Gravsphere_MaxForce_Patch));

			if (config.treaderSpawnChunkProbability != 1f)
				HarmonyHelper.setPatchEnabled(true, typeof(OptionalPatches.SeaTreader_SpawnChunksProb_Patch));
		}
#endif
	}
}