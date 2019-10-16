using Common.Config;

namespace GravTrapImproved
{
	[AddFieldsToConsole("gt")]
	class ModConfig: Config
	{
		[Options.Field(CustomActionType: typeof(CheckPatches))]
		public readonly bool testPatch = false;

		[FieldBounds(0f, 1f)]
		public readonly float treaderSpawnChunkProbability = 1f;
		
		public readonly bool useWheelClick = true;
		public readonly bool useWheelScroll = true;

		[FieldBounds(1, 500)]
		[Options.Field(CustomActionType: typeof(CheckPatches))]
		public readonly int maxObjects = 12;
		
		[FieldBounds(0, 900)]
		[Options.Field(CustomActionType: typeof(CheckPatches))]
		public readonly float maxForce = 15f;
		
		[FieldBounds(0, 900)]
		[Options.Field(CustomActionType: typeof(CheckPatches))]
		public readonly float maxRadius = 17f;

		class CheckPatches: Options.IFieldCustomAction { public void fieldCustomAction() => Main.checkOptionalPatches(); }
	}
}