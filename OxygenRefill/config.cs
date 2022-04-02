using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Configuration;

namespace OxygenRefill
{
	class ModConfig: Config
	{
		public readonly float multCapacity = 1.0f;

		[NoInnerFieldsAttrProcessing]
		public readonly Dictionary<TechType, float> tankCapacities = new()
		{
			{TechType.Tank,				200f},
			{TechType.DoubleTank,		400f},
			{TechType.PlasteelTank,		400f},
			{TechType.HighCapacityTank, 800f},
#if GAME_BZ
			{TechType.SuitBoosterTank,	400f},
#endif
		};

		public float getTankCapacity(GameObject tank) =>
			tankCapacities.TryGetValue(CraftData.GetTechType(tank), out float capacity)? capacity * multCapacity: 0;
	}

	class L10n: LanguageHelper
	{
		public const string ids_OxygenStation = "Oxygen refill station";
		public const string ids_OxygenStationDesc = "Oxygen refill station.";

		public const string ids_RefillOxygen = "Refill oxygen";
		public const string ids_RefillOxygenDesc = "Refill oxygen tank.";

		public static string ids_UseStation = "Use oxygen refill station";

		public static string ids_ToggleTankUsage = "toggle tank usage";
		public static string ids_TankIsNotUsed = "<color=#FF0000FF>Tank is not used</color>";
	}
}