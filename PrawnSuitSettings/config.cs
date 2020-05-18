using Common.Harmony;
using Common.Configuration;

namespace PrawnSuitSettings
{
	[AddToConsole("pss")]
	class ModConfig: Config
	{
		[Options.Hideable(typeof(Hider), "collision")]
		[Options.FinalizeAction(typeof(CollisionSelfDamage.SettingChanged))]
		public class CollisionSelfDamageSettings
		{
			class Hider: Options.Components.Hider.Simple
			{ public Hider(): base("collision", () => Main.config.collisionSelfDamage.enabled) {} }

			[Options.Field("Damage from collisions", "Damage for Prawn Suit from collisions with terrain and other objects")]
			[Field.Action(typeof(Hider))]
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
			public readonly bool enabled = false;

			[Options.Field("\tMinimum speed", "Prawn Suit minimum speed to get self damage from collision")]
			[Field.Range(0f, 50f)]
			[Options.Slider(defaultValue: 20f)]
			public readonly float speedMinimumForDamage = 20f;

			[Options.Field("\tMirrored damage fraction", "Fraction of total inflicted collision damage that goes to self damage")]
			[Field.Range(min: 0f)]
			[Options.Slider(defaultValue: 0.1f, maxValue: 1f, valueFormat: "{0:P0}")]
			public readonly float mirroredSelfDamageFraction = 0.1f;
		}
		public readonly CollisionSelfDamageSettings collisionSelfDamage = new CollisionSelfDamageSettings();


		[Options.Hideable(typeof(Hider), "arms_energy")]
		[Options.FinalizeAction(typeof(ArmsEnergyUsage.SettingChanged))]
		public class ArmsEnergyUsageSettings
		{
			class Hider: Options.Components.Hider.Simple
			{ public Hider(): base("arms_energy", () => Main.config.armsEnergyUsage.enabled) {} }

			[Options.Field("Arms additional energy usage", "Energy consuming for drill arm and grappling arm")]
			[Field.Action(typeof(Hider))]
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
			public readonly bool enabled = false;

			[Options.Field("\tDrill arm", "Using drill arm costs that much energy units per second")]
			[Field.Range(min: 0f)]
			[Options.Slider(defaultValue: 0.3f, maxValue: 2.0f, valueFormat: "{0:F1}", customValueType: typeof(Options.Components.SliderValue.ExactlyFormatted))]
			public readonly float drillArm = 0.3f;

			[Options.Field("\tGrappling arm (shoot)", "Shooting grappling hook costs that much energy units")]
			[Field.Range(min: 0f)]
			[Options.Slider(defaultValue: 0.5f, maxValue: 2.0f, valueFormat: "{0:F1}", customValueType: typeof(Options.Components.SliderValue.ExactlyFormatted))]
			public readonly float grapplingArmShoot = 0.5f;

			[Options.Field("\tGrappling arm (pull)", "Using grappling arm to pull Prawn Suit costs that much energy units per second")]
			[Field.Range(min: 0f)]
			[Options.Slider(defaultValue: 0.2f, maxValue: 2.0f, valueFormat: "{0:F1}", customValueType: typeof(Options.Components.SliderValue.ExactlyFormatted))]
			public readonly float grapplingArmPull = 0.2f;

			// vanilla energy usage
			[Field.Range(min: 0f)]
			public readonly float torpedoArm = 0f; // shooting with torpedo arm costs that much energy units

			[Field.Range(min: 0f)]
			public readonly float clawArm = 0.1f; // using claw arm costs that much energy units
		}
		public readonly ArmsEnergyUsageSettings armsEnergyUsage = new ArmsEnergyUsageSettings();

		[Options.Field("Propulsion arm 'ready' animation", "Whether propulsion arm should play animation when pointed to something pickupable")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool readyAnimationForPropulsionCannon = true;

		[Options.Field("Toggleable drill arm", "Whether you need to hold mouse button while using drill arm")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool toggleableDrillArm = false;

		[Options.Field("Auto pickup resources after drilling", "Drilled resources will be added to the Prawn Suit storage automatically")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool autoPickupDrillableResources = true;
	}
}