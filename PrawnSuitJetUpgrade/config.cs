using Common;
using Common.Configuration;

namespace PrawnSuitJetUpgrade
{
	[AddToConsole("ps_ju")]
	class ModConfig: Config
	{
		[Field.Range(min: 1f)] public readonly float increasedThrustReserve = 2.0f;
		[Field.Range(min: 0f)] public readonly float jetPowerAboveWater = 6.0f;

		[Field.Range(min: 0f)] public readonly float additionalThrustConsumptionAboveWater = 3.0f;
		[Field.Range(min: 0f)] public readonly float additionalPowerConsumptionAboveWater  = 0.2f;
	}

	class L10n: LanguageHelper
	{
		public const string ids_optimizerName = "Prawn suit thrusters optimizer";
		public const string ids_optimizerDesc = "Thrusters work longer before need to recharge.";

		public static readonly string ids_thrustersEfficiency = "Thrusters efficiency is {0:P0}";
	}
}