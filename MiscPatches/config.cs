using Common.Harmony;
using Common.Configuration;

namespace MiscPatches
{
	class ModConfig: Config
	{
		[Options.Field("Gameplay patches", "Reload in order to apply")]
		public readonly bool gameplayPatches = false;

		public readonly float torpedoPunchForce = 30; //real default is 70, but in code default is 30

		public readonly float flareBurnTime = 300; // default is 1800
		public readonly float flareIntensity = 3;  // default is 6

		public readonly int maxSlotsCountSeamoth = 8;
		public readonly int maxSlotsCountPrawnSuit = 4; // and +2 for arms

		public readonly float vehicleLightEnergyPerSec = 0.1f;
		public readonly UnityEngine.KeyCode toggleLightKey = UnityEngine.KeyCode.F;

		public readonly float continuousDamageCheckInterval = 5f;
		public readonly float minHealthPercentForContinuousDamage = 0.3f;
		public readonly float chanceForDamage = 0.3f;
		public readonly float additionalContinuousDamage = 1f; // absolute value

		public readonly bool additionalPropRepImmunity = true; // propulsion/repulsion cannon immunity for some additional objects

		public readonly bool  changeChargersSpeed   = true;
		public readonly bool  chargersAbsoluteSpeed = true;    // charge speed is not linked to capacity (default false)
		public readonly float batteryChargerSpeed   = 0.0015f; // 0.0015f
		public readonly float powerCellChargerSpeed = 0.0025f; // 0.0025f

		[AddToConsole("misc")]
		public class Debug
		{
			public readonly bool buildAnywhere = true;
			public readonly UnityEngine.KeyCode forceBuildAllowKey = UnityEngine.KeyCode.V;

			[Options.Field("Loot reroll")]
			[Options.Choice("None", 0, "x10", 10, "x100", 100)]
			[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
			public readonly int lootSpawnRerollCount = 0;

			[Field.Action(typeof(UpdateOptionalPatches))]
			public readonly bool propulsionCannonIgnoreLimits = false;

			[Field.Action(typeof(VFXController_SpawnFX_Patch.Purge))]
			[Field.Action(typeof(UpdateOptionalPatches))]
			public readonly bool keepParticleSystemsAlive = false;

			[Options.Field("Scanner room cheat")]
			[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
			public readonly bool scannerRoomCheat = false;

			public class FastStart
			{
				class Hider: Options.Components.Hider.Simple
				{ public Hider(): base("fast", () => Main.config.dbg.fastStart.enabled) {} }

				[Options.Field("Fast start")]
				[Field.Action(typeof(Hider))]
				[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
				public readonly bool enabled = false;

				[Options.Field("\tLoad escape pod")]
				[Options.Hideable(typeof(Hider), "fast")]
				public readonly bool loadEscapePod = false;

				public readonly string[] commandsAfterLoad = new string[]
				{
				};
			}
			public readonly FastStart fastStart = new FastStart();
		}
		public Debug dbg = new Debug();
	}
}