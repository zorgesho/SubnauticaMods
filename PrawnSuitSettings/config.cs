using System;
using Common.Configuration;

namespace PrawnSuitSettings
{
	[AddToConsole("pss")]
	[Options.Name("Prawn Suit Settings")]
	class ModConfig: Config
	{
		public readonly bool addOptionsToMenu = true;

		public void _refreshAfterLoad()
		{
			collisionSelfDamage._enabled_0 = collisionSelfDamage.enabled;
			armsEnergyUsage._enabled_1 = armsEnergyUsage.enabled;
		}

		public class CollisionSelfDamageSettings
		{
			[NonSerialized]
			[Options.Field("Damage from collisions")]
			[Field.CustomAction(typeof(CollisionSelfDamage.SettingChanged))]
			public bool _enabled_0 = false; // can't use just 'enabled' now to avoid name collision in mod options

			public bool enabled = false;

			public readonly float speedMinimumForDamage = 20f;
			public readonly float mirroredSelfDamageFraction = 0.1f;
		}
		public readonly CollisionSelfDamageSettings collisionSelfDamage = new CollisionSelfDamageSettings();

		public class ArmsEnergyUsageSettings
		{
			[NonSerialized]
			[Options.Field("Arms additional energy usage")]
			[Field.CustomAction(typeof(ArmsEnergyUsage.SettingChanged))]
			public bool _enabled_1 = false; // temporary workaround

			public bool enabled = false;

			public readonly float drillArm = 0.3f;
			public readonly float grapplingArmShoot = 0.5f;
			public readonly float grapplingArmPull = 0.2f;

			// vanilla energy usage
			public readonly float torpedoArm = 0f;
			public readonly float clawArm = 0.1f;
		}
		public readonly ArmsEnergyUsageSettings armsEnergyUsage = new ArmsEnergyUsageSettings();

		[Options.Field("Propulsion arm 'ready' animation")]
		public readonly bool readyAnimationForPropulsionCannon = true;

		[Options.Field("Toggleable drill arm")]
		public readonly bool toggleableDrillArm = false;

		[Options.Field("Auto pickup resources after drilling")]
		[Field.CustomAction(typeof(Common.HarmonyHelper.UpdateOptionalPatches))]
		public readonly bool autoPickupDrillableResources = true;
	}
}