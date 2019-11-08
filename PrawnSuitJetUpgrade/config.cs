using Common.Configuration;

namespace PrawnSuitJetUpgrade
{
	class ModConfig: Config
	{
		[Field.Bounds(Min: 1f)]
		public readonly float increasedThrustReserve = 2.0f;

		[Field.Bounds(Min: 0f)]
		public readonly float jetPowerAboveWater = 6.0f;

		[Field.Bounds(Min: 0f)]
		public readonly float additionalThrustConsumptionAboveWater = 3.0f;

		[Field.Bounds(Min: 0f)]
		public readonly float additionalPowerConsumptionAboveWater = 0.2f;
	}
}