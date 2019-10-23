using Common.Configuration;

namespace ConsoleImproved
{
	class ModConfig: Config
	{
		public readonly bool consoleEnabled = true;
		public readonly bool keepMessagesOnScreen = true;
		public readonly int  historySizeToSave = 100;
	}
}