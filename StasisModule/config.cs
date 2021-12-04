using Common.Configuration;

namespace StasisModule
{
	[Field.BindConsole("sts_mod")]
	class ModConfig: Config
	{
		[Field.Range(2f, 60f)]
		public readonly float stasisTime = 10f; // vanilla: min = 4, max = 20

		[Field.Range(1f, 25f)]
		public readonly float stasisRadius = 10f; // vanilla: min = 1, max = 10

		[Field.Range(0.5f, 60f)]
		public readonly float cooldown = 10f;

		[Field.Range(0f, 100f)]
		public readonly float energyCost = 5f;
	}
}