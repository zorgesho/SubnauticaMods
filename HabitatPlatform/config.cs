using Common.Configuration;

namespace HabitatPlatform
{
	[AddToConsole]
	class ModConfig: Config
	{
		public readonly float step = 1f;
		public readonly float stepAngle = 0.5f;
	}
}