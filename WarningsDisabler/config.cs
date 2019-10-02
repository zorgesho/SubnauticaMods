using Common.Config;

namespace WarningsDisabler
{
	[System.Serializable]
	[Options.Name("Warnings Disabler")]
	class Config: BaseConfig
	{
		[Options.Field("Disable Oxygen Warnings")]
		public bool disableOxygenWarnings = true;
	}
}