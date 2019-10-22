using Common;
using Common.Config;

namespace GravTrapImproved
{
	static public class Main
	{
		static internal readonly ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();

			//Options.init();
#if VER_1_2_0
			checkOptionalPatches();
#endif
		}

#if VER_1_2_0
		static internal void checkOptionalPatches()
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