using Common.Configuration;

namespace CustomHotkeys
{
	class ModConfig: Config
	{
		[AddToConsole("hotkeys")]
		public readonly bool disableDevTools = true;
	}
}