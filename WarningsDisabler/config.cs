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
	}
}