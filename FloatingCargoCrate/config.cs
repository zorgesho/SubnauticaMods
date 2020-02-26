using Common.Configuration;

namespace FloatingCargoCrate
{
	class ModConfig: Config
	{
		[Field.Range(1, 2)] public readonly int cargoModelType = 1;

		[Field.Range(1, 8)]  public readonly int storageWidth = 8;
		[Field.Range(1, 10)] public readonly int storageHeight = 8;

		[Field.Range(Min: 100f)] public readonly float crateMass = 600.0f;

		public readonly bool cheapBlueprint = true;
		public readonly bool experimentalFeaturesOn = true;

		[Field.Range(Min: 100f)] public readonly float crateMassEmpty = 400.0f;
		[Field.Range(Min: 100f)] public readonly float crateMassFull = 1200.0f;
	}
}