using Common.Configuration;

namespace SimpleModManager
{
	[Options.Name("Mod Manager")]
	class ModConfig: Config
	{
		public readonly string[] blacklist = new[]
		{
			"SimpleModManager",
			"Modding Helper",
			"ConsoleImproved",
			"CustomHotkeys"
		};
	}
}