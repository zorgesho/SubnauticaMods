using System.Collections.Generic;
using Common.Configuration;

namespace OxygenRefill
{
	class ModConfig: Config
	{
		public readonly float multCapacity = 1.0f;

		public readonly Dictionary<TechType, float> tankCapacities = new Dictionary<TechType, float>
		{
			{TechType.Tank, 111f},
			{TechType.DoubleTank, 222f},
			{TechType.PlasteelTank, 333f},
			{TechType.HighCapacityTank,  444f},
		};
	}
}