using Common.Configuration;

namespace PrawnSuitJetUpgrade
{
	class ModConfig: Config
	{
		[Field.Range(min: 1f)] public readonly float increasedThrustReserve = 2.0f;
		[Field.Range(min: 0f)] public readonly float jetPowerAboveWater = 6.0f;

		[Field.Range(min: 0f)] public readonly float additionalThrustConsumptionAboveWater = 3.0f;
		[Field.Range(min: 0f)] public readonly float additionalPowerConsumptionAboveWater  = 0.2f;
	}
}