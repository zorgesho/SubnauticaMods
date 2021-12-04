using Common.Configuration;

namespace StasisTorpedo
{
	[Field.BindConsole("sts_trp")]
	class ModConfig: Config
	{
		[Field.Range(2f, 60f)]
		public readonly float stasisTime = 15f; // vanilla: min = 4, max = 20

		[Field.Range(1f, 25f)]
		public readonly float stasisRadius = 10f; // vanilla: min = 1, max = 10
	}
}