using Common.Configuration;

namespace GravTrapImproved
{
	class ModConfig: Config
	{
		const bool rangesLimited =
#if DEBUG
			false;
#else
			true;
#endif
		public readonly bool useWheelScroll = true;
		public readonly bool useWheelClick = false;

		public readonly float treaderSpawnChunkProbability = 1f;

		public readonly bool mk2Enabled = true;

		[Field.Range(0, 30)] public readonly int mk2FragmentCountToUnlock = 4; // unlock with vanilla gravtrap if zero

		[Field.Range(12, rangesLimited? 24: 1000)]
		[AddToConsole("gt_mk2")]
		public readonly int mk2MaxObjectCount = 18; // default: 12

		[Field.Range(15, rangesLimited? 100: 1000)]
		[AddToConsole("gt_mk2")]
		public readonly float mk2MaxForce = 25f; // default: 15f

		[Field.Range(17, rangesLimited? 50: 1000)]
		[AddToConsole("gt_mk2")]
		[Field.Action(typeof(GravTrapMK2Patches.UpdateRadiuses))]
		public readonly float mk2MaxRadius = 25f; // default: 17f
	}
}