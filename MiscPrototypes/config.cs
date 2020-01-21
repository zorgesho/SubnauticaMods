using Common.Configuration;

namespace MiscPrototypes
{
	class ModConfig: Config
	{
		public readonly int field = 42;

		public readonly float maxPowerOnBatteries = 50f;

		[Options.Field("Short description of choice")]
		[Options.Choice("Choice 1", "Choice 2", "Choice 3")]
		public readonly int choice1 = 0;

		[Options.Field("Long string with description of this choice")]
		[Options.Choice("Choice 1", "Choice 2", "Choice 3")]
		public readonly int choice2 = 0;

		[Options.Field("Short description of slider")]
		[Field.Bounds(0, 1000)]
		public readonly float slider1 = 500f;

		[Options.Field("Long string with description of this slider")]
		[Field.Bounds(0, 100)]
		public readonly float slider2 = 50f;

		[Options.Field("Short description of checkbox")]
		public readonly bool toggle1 = false;

		[Options.Field("Really long string with description of this checkbox")]
		public readonly bool toggle2 = true;


		[Options.Field("Key description")]
		public readonly UnityEngine.KeyCode keybind1 = UnityEngine.KeyCode.F;

		[Options.Field("Key ------------------------------------ description")]
		public readonly UnityEngine.KeyCode keybind2 = UnityEngine.KeyCode.B;


		public readonly float scale = 1.0f;
		public readonly float posX = 15f;
		public readonly float posY = -13f;
		
		public readonly float posX2 = 45;//22.5f;
		public readonly float pivotX = 0.25f;
		public readonly float pivotY = 0.5f;

		public readonly float alpha = 0.2f;
		public readonly float delay = 0.05f;
	}
}