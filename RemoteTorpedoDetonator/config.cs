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
}