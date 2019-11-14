namespace FloatingCargoCrate
{
	[System.Serializable]
	public class Config
	{
		public int cargoModelType = 1;
		public int storageWidth = 8;
		public int storageHeight = 8;
		public float crateMass = 600.0f;
		public bool cheapBlueprint = true;
		public bool experimentalFeaturesOn = true;
		public float crateMassEmpty = 400.0f;
		public float crateMassFull = 1200.0f;
	}
}