using Common;
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

		public readonly float vehicleLightEnergyPerSec = 0.1f;
		public readonly UnityEngine.KeyCode toggleLightKey = UnityEngine.KeyCode.F;

		public readonly float continuousDamageCheckInterval = 5f;
		public readonly float minHealthPercentForContinuousDamage = 0.3f;
		public readonly float chanceForDamage = 0.3f;
		public readonly float additionalContinuousDamage = 1f; // absolute value

		public readonly bool additionalPropRepImmunity = true; // propulsion/repulsion cannon immunity for some additional objects

		public readonly bool  changeChargersSpeed   = true;
		public readonly bool  chargersAbsoluteSpeed = true;    // charge speed is not linked to capacity (default false)
		public readonly float batteryChargerSpeed   = 0.0015f; // 0.0015f
		public readonly float powerCellChargerSpeed = 0.0025f; // 0.0025f

		// debug stuff
		[Field.Range(min: 0)]
		[AddToConsole("misc")]
		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly int loolSpawnRerollCount = 0;
	}
}