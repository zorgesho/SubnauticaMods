using Common.Config;

namespace GravTrapImproved
{
	[System.Serializable]
	class ModConfig: Config
	{
		public readonly float treaderSpawnChunkProbability = 1f;
		
		public readonly bool useWheelClick = true;

		public readonly bool useWheelScroll = true;
	}
}