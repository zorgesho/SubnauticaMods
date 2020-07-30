using Common;
using Common.Configuration;

namespace UITweaks
{
	[Field.BindConsole("ui")]
	class ModConfig: Config
	{
		public readonly float _tooltipOffsetX = 30f;
	}

	class L10n: LanguageHelper
	{
		public static readonly string ids_changeAmount = "change amount";
	}
}