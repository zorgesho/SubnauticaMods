using Common.Configuration;

namespace CustomHotkeys
{
	[AddToConsole("hotkeys")]
	class ModConfig: Config
	{
		public readonly bool disableDevTools = true;
		
		public readonly float warpStep = 2;
	}
}