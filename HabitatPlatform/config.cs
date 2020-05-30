using Common.Configuration;

namespace HabitatPlatform
{
	[Field.BindConsole]
	class ModConfig: Config
	{
		public readonly float step = 1f;
		public readonly float stepAngle = 0.5f;

		public readonly int xxx = 4;
		public readonly int zzz = 3;
	}
}