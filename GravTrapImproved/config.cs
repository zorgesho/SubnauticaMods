﻿using Common.Config;

namespace GravTrapImproved
{
	[System.Serializable]
	class ModConfig: Config
	{
		public readonly float treaderSpawnChunkProbability = 1f;

		public readonly bool useWheelClick = true;

		public readonly bool useWheelScroll = true;

		[FieldBounds(1, 500)]
		[Options.Field]
		public readonly int maxAttractedObjects = 12;
		
		[FieldBounds(0, 900)]
		[Options.Field]
		public readonly float maxForce = 15f;
		
		[FieldBounds(0, 900)]
		[Options.Field]
		public readonly float maxRadius = 17f;
	}
}