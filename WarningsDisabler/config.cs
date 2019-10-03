using System;
using Common.Config;

namespace WarningsDisabler
{
	[Serializable]
	[Options.Name("Warnings Disabler")]
	class Config: BaseConfig
	{
		[Options.Field("Disable Oxygen Warnings", typeof(OxygenWarnings.OxygenWarningsCustomAction))]
		public bool disableOxygenWarnings = true;
		
		[Options.Field]
		public bool disableDepthWarnings = true;

		[Options.Field]
		public bool disablePowerWarnings = true;
		
		[Options.Field]
		public bool disableFoodWaterWarnings = true;
		
		[Options.Field]
		public bool disableWelcomesMessages = true;
	}
}