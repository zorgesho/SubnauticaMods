using Common.Configuration;

namespace MiscPrototypes
{
	class ModConfig: Config
	{
		public readonly int field = 42;

		public readonly float maxPowerOnBatteries = 50f;
	}
}