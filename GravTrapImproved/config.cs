using Common;
using Common.Config;

namespace GravTrapImproved
{
	class ModConfig: Config
	{
		[AddToConsole("gt")]
		[Options.Field]
		[CfgField.CustomAction(typeof(TestAction))]
		public readonly bool testPatch = false;

		[FieldBounds(0f, 1f)]
		public readonly float treaderSpawnChunkProbability = 1f;
		
		public readonly bool useWheelClick = true;
		public readonly bool useWheelScroll = true;

		[AddToConsole("gt")]
		[FieldBounds(1, 500)]
		[Options.Field]
		[CfgField.CustomAction(typeof(CheckPatches))]
		public readonly int maxObjects = 12;
		
		[AddToConsole("gt")]
		[FieldBounds(0, 900)]
		[Options.Field]
		[CfgField.CustomAction(typeof(CheckPatches))]
		public readonly float maxForce = 15f;
		
		[AddToConsole("gt")]
		[FieldBounds(0, 900)]
		[Options.Field]
		[CfgField.CustomAction(typeof(CheckPatches))]
		public readonly float maxRadius = 17f;

		class CheckPatches: CfgField.ICustomAction { public void customAction() => Main.checkOptionalPatches(); }
		
		class TestAction: CfgField.ICustomAction { public void customAction() => "TEST CUSTOM ACTION".onScreen(); }
	}
}