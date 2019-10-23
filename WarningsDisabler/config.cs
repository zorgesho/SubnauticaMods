using System;
using Common.Configuration;

namespace WarningsDisabler
{
	[Serializable]
	[Options.Name("Warnings & messages <color=#999999FF>(uncheck boxes to disable)</color>")]
	class ModConfig: Config
	{
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