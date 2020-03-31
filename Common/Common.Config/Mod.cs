using Common.Configuration;

namespace Common
{
	static partial class Mod
	{
		public static C init<C>() where C: Config, new()
		{
			init();
			return Config.tryLoad<C>();
		}
	}
}