using Common.Config;

namespace ConsoleImproved
{
	class ModConfig: Config
	{
		public readonly bool consoleEnabled = true;
		public readonly int  historySizeToSave = 100;
	}
}