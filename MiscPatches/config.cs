using Common.Configuration;

namespace MiscPatches
{
	class ModConfig: Config
	{
		public readonly float torpedoPunchForce = 30; //real default is 70, but in code default is 30
		
		public readonly float flareBurnTime = 300; // default is 1800
		public readonly float flareIntensity = 3;  // default is 6

		public readonly UnityEngine.KeyCode forceBuildAllowKey = UnityEngine.KeyCode.V;

		public readonly int maxSlotsCountSeamoth = 8;
		public readonly int maxSlotsCountPrawnSuit = 4; // and +2 for arms
	}
}