using Common;
using Common.Harmony;
using Common.Configuration;

namespace UITweaks
{
	[Field.BindConsole("ui")]
	class ModConfig: Config
	{
		public readonly float _tooltipOffsetX = 30f; // TODO remove

		[Options.Field] // TODO name & tooltip
		[Field.Action(typeof(UpdateOptionalPatches))] // TODO use FinalizeAction
		public readonly bool bulkCrafting = true;
	}

	class L10n: LanguageHelper
	{
		public static readonly string ids_changeAmount = "change amount";
	}
}