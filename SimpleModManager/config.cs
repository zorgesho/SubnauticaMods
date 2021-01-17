using Common.Configuration;
using Common.Configuration.Actions;

namespace SimpleModManager
{
	[Options.Name("Mod Manager")]
	[Options.CustomOrder(modIDBefore)]
	class ModConfig: Config
	{
		public const string modIDBefore = "SMLHelper";

		[Options.Field("Show hidden mods")]
		[Field.Action(typeof(CallMethod), nameof(_showHiddenMods))]
		public readonly bool showHiddenMods = false;
		void _showHiddenMods() => Options.Components.Hider.setVisible("hidden-mod", showHiddenMods);

		[Options.Field("Show blacklisted mods")]
		[Field.Action(typeof(CallMethod), nameof(_showBlacklisterMods))]
		public readonly bool showBlacklistedMods = false;
		void _showBlacklisterMods() => Options.Components.Hider.setVisible("blacklist-mod", showBlacklistedMods);

		public readonly string[] blacklist = new[]
		{
			"SimpleModManager",
			"SMLHelper",
			"ConsoleImproved",
			"CustomHotkeys"
		};
	}
}