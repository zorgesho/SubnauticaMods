using Common.Configuration;

namespace TrfHabitatBuilder
{
	class ModConfig: Config
	{
		public readonly bool bigInInventory = false;
		public readonly bool removeVanillaBuilder = false;
		public readonly bool slowLoopAnim = false;

#if DEBUG
		[AddToConsole]
		public readonly float forcedBuildTime = 1.0f;
#endif
	}
}