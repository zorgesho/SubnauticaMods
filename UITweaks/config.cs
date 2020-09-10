using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace UITweaks
{
	[Field.BindConsole("ui")]
	class ModConfig: Config
	{
		public class BulkCrafting
		{
			[Options.Field] // TODO name & tooltip
			[Field.Action(typeof(UpdateOptionalPatches))] // TODO use FinalizeAction
			public readonly bool enabled = true;

			[Field.Action(typeof(UpdateOptionalPatches))]
			public readonly bool changeCraftDuration = true;

			public readonly bool changePowerConsumption = true;
		}
		public readonly BulkCrafting bulkCrafting = new BulkCrafting();

		[Options.Hideable(typeof(Hider), "pda")]
		public class PDATweaks
		{
			class Hider: Options.Components.Hider.Simple
			{ public Hider(): base("pda", () => Main.config.pdaTweaks.enabled) {} }

			[Options.Field] // TODO name & tooltip
			[Field.Action(typeof(Hider))]
			[Field.Action(typeof(UpdateOptionalPatches))] // TODO use FinalizeAction
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			public readonly bool enabled = true;

			[Options.Field] // TODO name & tooltip
			public readonly bool allowClearNotifications = true;

			[Options.Field] // TODO name & tooltip
			public readonly bool showItemCount = true;

			[Options.Field] // TODO name & tooltip
			public readonly bool tabHotkeysEnabled = true;

			[Options.Field] // TODO name & tooltip
			[Field.Action(typeof(UpdateOptionalPatches))] // TODO use FinalizeAction
			public readonly bool pingTogglesEnabled = true;

			[Field.Reloadable]
			[NoInnerFieldsAttrProcessing]
			public readonly Dictionary<PDATab, KeyCode> tabHotkeys = new Dictionary<PDATab, KeyCode>()
			{
				{ PDATab.Inventory,		KeyCode.Alpha1 },
				{ PDATab.Journal,		KeyCode.Alpha2 },
				{ PDATab.Ping,			KeyCode.Alpha3 },
				{ PDATab.Gallery,		KeyCode.Alpha4 },
				{ PDATab.Log,			KeyCode.Alpha5 },
				{ PDATab.Encyclopedia,	KeyCode.Alpha6 },
			};
		}
		public readonly PDATweaks pdaTweaks = new PDATweaks();

		[Options.Field]
		[Field.Action(typeof(UpdateOptionalPatches))] // TODO use FinalizeAction
		public readonly bool builderMenuTabHotkeysEnabled = true;

		public readonly bool showToolbarHotkeys = true;
	}

	class L10n: LanguageHelper
	{
		public static readonly string ids_changeAmount = "change amount";
		public static readonly string ids_clearNotifications = "clear notifications";
	}
}