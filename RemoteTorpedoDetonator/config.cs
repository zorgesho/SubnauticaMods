using Common;
using Common.Configuration;

namespace RemoteTorpedoDetonator
{
	[AddToConsole("rtd")]
	class ModConfig: Config
	{
		[Field.Range(min: 1f)]
		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly float torpedoSpeed = 10f;

		[Field.Range(min: 0f)]
		public readonly float torpedoCooldown = 5f;

		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly bool homingTorpedoes = true;

		public readonly bool showHotkeyMessage = true;
		public readonly GameInput.Button hotkey = GameInput.Button.Reload;

		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly bool cheatInfiniteTorpedoes = false;
	}

	class L10n: LanguageHelper
	{
		public const string ids_detonatorName = "Remote torpedo detonator";
		public const string ids_detonatorDesc = "Allows detonate launched torpedoes remotely. Seamoth/Prawn compatible.";

		public static readonly string ids_enterVehicleMessage = "Press <color=#ADF8FFFF>{0}</color> to detonate torpedoes remotely.";
	}
}