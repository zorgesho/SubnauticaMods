using System;
using Common.Config;

namespace WarningsDisabler
{
	[Serializable]
	[Options.Name("Warnings & messages <color=#999999FF>(uncheck boxes to disable)</color>")]
	class Config: BaseConfig
	{
		[Options.Field("Oxygen warnings", typeof(OxygenWarnings.HideOxygenHint))]
		public bool oxygenWarningsEnabled = true;
		
		[Options.Field("Depth warnings")]
		public bool depthWarningsEnabled = true;

		[Options.Field("Habitat power warnings")]
		public bool powerWarningsEnabled = true;
		
		[Options.Field("Food and water warnings")]
		public bool foodWaterWarningsEnabled = true;
		
		[Options.Field("Welcome messages")]
		public bool welcomeMessagesEnabled = true;
	}
}