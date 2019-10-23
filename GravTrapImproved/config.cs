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
		[FieldBounds(1, 50)]
		[Field.CustomAction(typeof(CheckPatches))]
		public readonly int maxObjects = 12;
		
		[AddToConsole("gt")]
		[FieldBounds(0, 100)]
		[Field.CustomAction(typeof(CheckPatches))]
		public readonly float maxForce = 15f;
		
		[AddToConsole("gt")]
		[FieldBounds(0, 50)]
		[Field.CustomAction(typeof(CheckPatches))]
		public readonly float maxRadius = 17f;

		class CheckPatches: Field.ICustomAction { public void customAction() => Main.checkOptionalPatches(); }
#endif
	}
}