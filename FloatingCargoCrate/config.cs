using Common;
using Common.Configuration;

namespace FloatingCargoCrate
{
	[AddToConsole("fcc")]
	class ModConfig: Config
	{
		[Field.Range(1, 2)] public readonly int cargoModelType = 1;

		[Field.Range(1, 8)]  public readonly int storageWidth = 8;
		[Field.Range(1, 10)] public readonly int storageHeight = 8;

		[Field.Range(min: 100f)] public readonly float crateMass = 600.0f;

		[AddToConsoleAttribute.Skip] public readonly bool cheapBlueprint = true;
		[AddToConsoleAttribute.Skip] public readonly bool experimentalFeaturesOn = true;

		[Field.Range(min: 100f)] public readonly float crateMassEmpty = 400.0f;
		[Field.Range(min: 100f)] public readonly float crateMassFull = 1200.0f;
	}


	class L10n: LanguageHelper
	{
		public const string ids_crateName = "Floating cargo crate";
		public const string ids_crateDesc = "Big cargo crate that floats and maintains position in the water.";

		public static readonly string ids_hoverText = "Open crate";
		public static readonly string ids_storageLabel = "CRATE";

		public static readonly string ids_beaconAttached = "Beacon attached";
		public static readonly string ids_removeBeaconFirst = "You need to remove beacon first";
		public static readonly string ids_attachBeaconToCrate = "\nAttach beacon to crate ({0})";
	}
}