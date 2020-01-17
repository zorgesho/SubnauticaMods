using Common;
using Common.Configuration;

namespace RemoteTorpedoDetonator
{
	class ModConfig: Config
	{
		[Field.Bounds(Min: 1f)]
		public readonly float torpedoSpeed = 10f;

		[Field.Bounds(Min: 0f)]
		public readonly float torpedoCooldown = 5f;

		public readonly bool  homingTorpedoes = true;

		public readonly bool  showHotkeyMessage = true;
		public readonly GameInput.Button hotkey = GameInput.Button.Reload;
	}

	class L10n: LanguageHelper
	{
		public const string ids_detonatorName = "Remote torpedo detonator";
		public const string ids_detonatorDesc = "Allows detonate launched torpedoes remotely. Seamoth/Prawn compatible.";

		public static string ids_enterVehicleMessage = "Press <color=#ADF8FFFF>{0}</color> to detonate torpedoes remotely.";
	}
}