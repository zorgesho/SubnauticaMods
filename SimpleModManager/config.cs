using Common.Configuration;

namespace SimpleModManager
{
	[Options.Name("Mod Manager")]
	class ModConfig: Config
	{
		class ShowHiddenMods: Field.IAction
		{ public void action() => Options.Components.Hider.setVisible("hidden-mod", Main.config.showHiddenMods); }

		class ShowBlacklistedMods: Field.IAction
		{ public void action() => Options.Components.Hider.setVisible("blacklist-mod", Main.config.showBlacklistedMods); }

		[Options.Field("Show hidden mods")]
		[Field.Action(typeof(ShowHiddenMods))]
		public readonly bool showHiddenMods = false;

		[Options.Field("Show blacklisted mods")]
		[Field.Action(typeof(ShowBlacklistedMods))]
		public readonly bool showBlacklistedMods = false;

		public readonly string[] blacklist = new[]
		{
			"SimpleModManager",
			"Modding Helper",
			"ConsoleImproved",
			"CustomHotkeys"
		};
	}
}