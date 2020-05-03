using Common.Configuration;

namespace Common
{
	static partial class Mod
	{
		public static C init<C>() where C: Config, new()
		{
			init();
			return loadConfig<C>();
		}


		public static C loadConfig<C>(string name = Config.defaultName, Config.LoadOptions loadOptions = Config.LoadOptions.Default) where C: Config, new()
		{
			C config = Config.tryLoad<C>(name, loadOptions);

			if (config == null)
			{
				addCriticalMessage($"Error while loading <color=orange>{name}</color>, using default now! ({Config.lastError})");
				config = Config.tryLoad<C>(null, loadOptions);
			}

			return config;
		}
	}
}