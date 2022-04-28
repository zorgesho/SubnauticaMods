using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Configuration;
using Common.Configuration.Actions;

namespace UITweaks
{
	using StorageTweaks;

#if DEBUG
	[Options.CustomOrder("QM")]
#endif
	[Field.BindConsole("ui")]
	class ModConfig: Config
	{
		[Options.Hideable(typeof(Hider), "bulk")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public class BulkCrafting
		{
			class Hider: Options.Components.Hider.Simple
			{ public Hider(): base("bulk", () => Main.config.bulkCrafting.enabled) {} }

			[Options.Field("Bulk crafting", "Ability to change the craft amount for the item using mouse wheel on the crafting tooltip")]
			[Field.Action(typeof(Hider))]
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			public readonly bool enabled = true;

			[Options.Field("\tChange craft duration", "Increase the duration for crafting the items based on the craft amount")]
			public readonly bool changeCraftDuration = true;

			[Options.Field("\tChange power consumption", "Increase the power consumption for crafting the items based on the craft amount")]
			public readonly bool changePowerConsumption = true;

			[Options.Field("\tFaster animations for the items", "Ingredients for crafting and crafted items will be fly from/to inventory faster, based on the craft amount")]
			public readonly bool inventoryItemsFasterAnim = true;
		}
		public readonly BulkCrafting bulkCrafting = new();

		[Options.Hideable(typeof(Hider), "pda")]
		public class PDATweaks
		{
			class Hider: Options.Components.Hider.Simple
			{ public Hider(): base("pda", () => Main.config.pdaTweaks.enabled) {} }

			[Options.Field("PDA tweaks")]
			[Field.Action(typeof(Hider))]
			[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			public readonly bool enabled = true;

			[Options.Field("\tToggles for beacons", "Additional buttons in the Beacon Manager for toggling beacons and signals based on their color")]
			[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
			public readonly bool pingTogglesEnabled = true;

			[Options.Field("\tAllow to clear notifications", "Notifications on the PDA tabs can be cleared with right mouse click")]
			public readonly bool allowClearNotifications = true;

			[Options.Field("\tTab hotkeys", "Switch between the tabs in the PDA with 1-6 keys (or custom keys)")]
			public readonly bool tabHotkeysEnabled = true;

			[Options.Field("\tShow item count", "Show number of items for the each tab on the tab tooltip")]
			public readonly bool showItemCount = true;

			[Field.Reloadable]
			[NoInnerFieldsAttrProcessing]
			public readonly Dictionary<PDATab, KeyCode> tabHotkeys = new()
			{
				{ PDATab.Inventory,		KeyCode.Alpha1 },
				{ PDATab.Journal,		KeyCode.Alpha2 },
				{ PDATab.Ping,			KeyCode.Alpha3 },
				{ PDATab.Gallery,		KeyCode.Alpha4 },
				{ PDATab.Log,			KeyCode.Alpha5 },
				{ PDATab.Encyclopedia,	KeyCode.Alpha6 },
			};
		}
		public readonly PDATweaks pdaTweaks = new();

		[Options.Hideable(typeof(Hider), "storage")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public class StorageTweaks
		{
			class Hider: Options.Components.Hider.Simple
			{ public Hider(): base("storage", () => Main.config.storageTweaks.enabled) {} }

			class ContentsInfoTweakHider: Options.Components.Hider.IVisibilityChecker, Field.IAction
			{
				public bool visible => StorageContentsInfo.tweakEnabled;

				public void action()
				{
					Options.Components.Hider.setVisible($"{nameof(storageTweaks)}.{nameof(showMaxItemCount)}", visible);
					Options.Components.Hider.setVisible($"{nameof(storageTweaks)}.{nameof(showSlotsInfo)}", visible);
				}
			}

			class AutonameTweakHider: Options.Components.Hider.IVisibilityChecker, Field.IAction
			{
				public bool visible => StorageLabelFixers.tweakEnabled;

				public void action() =>
					Options.Components.Hider.setVisible($"{nameof(storageTweaks)}.{nameof(autoname)}", visible);
			}

			class AutonameItemCountHider: Options.Components.Hider.IVisibilityChecker, Field.IAction
			{
				public bool visible => StorageAutoname.tweakEnabled;

				public void action() =>
					Options.Components.Hider.setVisible($"{nameof(storageTweaks)}.{nameof(autonameMaxItemCount)}", visible);
			}

			[Options.Field("Storage tweaks")] // TODO tooltip ?
			[Field.Action(typeof(Hider))]
			[Options.FinalizeAction(typeof(StorageAutoname.UpdateLabels))]
			[Options.FinalizeAction(typeof(StorageActions.UpdateStorages))]
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			public readonly bool enabled = true;

			[Options.Field("\t\"All-in-one\" actions", "TODO")] // TODO tooltip
			[Options.FinalizeAction(typeof(StorageActions.UpdateStorages))]
			public readonly bool allInOneActions = true;

			[Options.Field("\tShow storage contents info", "TODO")] // TODO tooltip
			[Field.Action(typeof(ContentsInfoTweakHider))]
			public readonly bool showContentsInfo = true;

			[Options.Field("\t\tMax item count", "TODO")] // TODO tooltip
			[Options.Slider(minValue: 0, maxValue: 10, defaultValue: 5)]
			[Options.Hideable(typeof(ContentsInfoTweakHider), "storage")]
			[Options.FinalizeAction(typeof(StorageContentsInfo.InvalidateCache))]
			public readonly int showMaxItemCount = 5;

			[Options.Field("\t\tShow free slots count", "TODO")] // TODO tooltip
			[Options.Hideable(typeof(ContentsInfoTweakHider), "storage")]
			[Options.FinalizeAction(typeof(StorageContentsInfo.InvalidateCache))]
			public readonly bool showSlotsInfo = true;

			[Options.Field("\tMultiline labels", "TODO (restart needed)")] // TODO tooltip
			[Field.Action(typeof(AutonameTweakHider))]
			[Field.Action(typeof(AutonameItemCountHider))]
			[Options.FinalizeAction(typeof(StorageAutoname.UpdateLabels))]
			public readonly bool multilineLabels = true;

			[Options.Field("\tStorage auto-naming", "TODO")] // TODO tooltip
			[Field.Action(typeof(AutonameItemCountHider))]
			[Options.Hideable(typeof(AutonameTweakHider), "storage")]
			[Options.FinalizeAction(typeof(StorageAutoname.UpdateLabels))]
			public readonly bool autoname = true;

			[Options.Field("\t\tMax item count", "TODO")] // TODO tooltip
			[Options.Slider(minValue: 1, maxValue: 5, defaultValue: 3)]
			[Options.Hideable(typeof(AutonameItemCountHider), "storage")]
			[Options.FinalizeAction(typeof(StorageAutoname.UpdateLabels))]
			public readonly int autonameMaxItemCount = 3;
		}
		public readonly StorageTweaks storageTweaks = new();

#if GAME_SN
		class HideRenameBeacons: Options.Components.Hider.IVisibilityChecker
		{ public bool visible => !Main.config.oldRenameBeaconsModActive; }

		[System.NonSerialized]
		bool oldRenameBeaconsModActive = false;

		[Options.Hideable(typeof(HideRenameBeacons))]
#endif
		[Options.Field("Rename beacons in the inventory", "Use middle mouse button (or custom hotkey) to rename beacons that are in the inventory")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public bool renameBeacons = true;
#if GAME_BZ
		[Options.Field("Switch targets for mineral detectors", "Use mouse wheel while hovering over a mineral detector in the inventory to switch between target objects")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public bool switchMetalDetectorTarget = true;
#endif
		[Options.Field("Hotkeys for builder menu tabs", "Switch between the tabs in the builder menu with 1-5 keys")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool builderMenuTabHotkeysEnabled = true;

		[Options.Field("Show save slot ID", "Show save slot ID on the load buttons")]
		[Field.Action(typeof(UpdateOptionalPatches), typeof(MiscTweaks.MainMenuLoadPanel_UpdateLoadButtonState_Patch))]
		public readonly bool showSaveSlotID = true;

		[Options.Field("Hide messages while loading", "Don't show messages that are added during loading the game")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool hideMessagesWhileLoading = true;

		const float defaultSpacing = 15f;

		[Options.Field("Options spacing", "Vertical spacing between options in the 'Mods' options tab")]
		[Field.Action(typeof(CallMethod), nameof(setOptionsSpacing))]
		[Options.Choice("Default", defaultSpacing, "Tight", 10f, "Compact", 5f)]
		readonly float optionsSpacing = defaultSpacing;
		void setOptionsSpacing() => Options.Utils.setOptionsSpacing(optionsSpacing);

		public readonly KeyCode renameBeaconsKey = KeyCode.None; // using middle mouse button by default
		public readonly bool showToolbarHotkeys = false;

		protected override void onLoad()
		{
#if GAME_SN
			if (Mod.isModEnabled("RenameBeacons"))
			{
				oldRenameBeaconsModActive = true;
				renameBeacons = false;
				Mod.addCriticalMessage(L10n.str(L10n.ids_modMerged), color: "yellow");
			}
#endif
			if (optionsSpacing != defaultSpacing)
				Options.Utils.setOptionsSpacing(optionsSpacing);
		}
	}

	class L10n: LanguageHelper
	{
		public static readonly string ids_bulkCraftChangeAmount = "change amount";
		public static readonly string ids_PDAClearNotifications = "clear notifications";

		public static readonly string ids_beaconName = "Name";
		public static readonly string ids_beaconRename = "rename";

		public static readonly string ids_freeSlots = "Free slots: {0}/{1}";
		public static readonly string ids_fullContainer = "Full";
		public static readonly string ids_otherItems = " and other items";
#if GAME_SN
		public static readonly string ids_modMerged = "<b>RenameBeacons</b> mod is now merged into <b>UI Tweaks</b> mod, you can safely delete it.";
#elif GAME_BZ
		public static readonly string ids_metalDetectorTarget = "Target: ";
		public static readonly string ids_metalDetectorSwitchTarget = "switch target";
#endif
	}
}