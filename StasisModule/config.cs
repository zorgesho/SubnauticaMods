using Common.Configuration;

namespace StasisModule
{
	[Field.BindConsole("st_mod")]
	class ModConfig: Config
	{
		[Field.Range(2f, 60f)]
		public readonly float stasisTime = 15f; // vanilla: min = 4, max = 20

		[Field.Range(1f, 25f)]
		public readonly float stasisRadius = 10f; // vanilla: min = 1, max = 10

		public readonly float cooldown = 2f; // TODO
		public readonly float energyCost = 1f; // TODO
	}
}