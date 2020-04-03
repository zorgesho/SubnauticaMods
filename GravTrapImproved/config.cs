using Common;
using Common.Configuration;

namespace GravTrapImproved
{
	class ModConfig: Config
	{
		public readonly bool useWheelScroll = true;
		public readonly bool useWheelClick = false;

		public readonly float treaderSpawnChunkProbability = 1f;

#if VER_1_2_0
		[AddToConsole("gt")]
		[Field.Range(1, 50)]
		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly int maxObjects = 12;

		[AddToConsole("gt")]
		[Field.Range(0, 100)]
		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly float maxForce = 15f;

		[AddToConsole("gt")]
		[Field.Range(0, 50)]
		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly float maxRadius = 17f;
#endif
	}
}