using Common;
using Common.Configuration;

namespace HabitatPlatform
{
	[Field.BindConsole("hbpl")]
	class ModConfig: Config
	{
		public readonly float stepMove = 0.1f;
		public readonly float stepRotate = 0.5f;

		public readonly bool chargeCameras = true;
		public readonly bool ignoreEnginesColliders = Mod.isDevBuild;

#if DEBUG
		public readonly bool dbgVisibleFoundations = false;
		public readonly bool dbgPrintColliders = false;
		public readonly bool dbgFastPlatformBuild = true;
		public readonly bool dbgKinematicForBuilded = true;
#endif
	}
}