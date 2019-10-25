using Common.Configuration;

namespace WarningsDisabler
{
	[Options.Name("Warnings & messages <color=#CCCCCCFF>(uncheck boxes to disable)</color>")]
	class ModConfig: Config
	{
		public readonly bool addOptionsToMenu = true;

		[Options.Field("Oxygen warnings")]
		[Field.CustomAction(typeof(OxygenWarnings.HideOxygenHint))]
		public readonly bool oxygenWarningsEnabled = true;

		[Options.Field("Food and water warnings")]
		public readonly bool foodWaterWarningsEnabled = true;

		[Options.Field("Depth warnings")]
		public readonly bool depthWarningsEnabled = true;

		[Options.Field("Habitat power warnings")]
		public readonly bool powerWarningsEnabled = true;

		[Options.Field("Welcome messages")]
		public readonly bool welcomeMessagesEnabled = true;
	}
}