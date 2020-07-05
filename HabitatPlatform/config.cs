using Common.Configuration;

namespace HabitatPlatform
{
	[Field.BindConsole("hb_p")]
	class ModConfig: Config
	{
		public readonly bool dbgVisibleFoundations = false;
		public readonly bool dbgPrintColliders = false;

		public readonly float stepMove = 1f;
		public readonly float stepRotate = 0.5f;
	}
}