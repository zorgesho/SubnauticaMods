using Common.Config;

namespace WarningsDisabler
{
	[System.Serializable]
	public class Config: BaseConfig
	{
		public bool disableOxygenWarnings = true;
	}
}