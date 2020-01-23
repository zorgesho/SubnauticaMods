using System;
using System.IO;

namespace Common.Configuration
{
	partial class Config
	{
		public static Config main { get; private set; } = null;

		public enum LoadOptions
		{
			None = 0,
			ProcessAttributes = 1,
			SetAsMainConfig = 2,
			ForcedLoad = 4,
			Default = ProcessAttributes | SetAsMainConfig
		}

		static readonly bool loadFromFile = // can be overrided by LoadOptions.ForcedLoad
#if (DEBUG && !LOAD_CONFIG)
			false;
#else
			true;
#endif
		string configPath;

		protected Config() {}

		// try to load config from mod folder. If file not found, create default config and save it to that path
		public static C tryLoad<C>(string localPath = "config.json", LoadOptions loadOptions = LoadOptions.Default) where C: Config, new()
		{
			string configPath = localPath != null? Paths.modRootPath + localPath: null;
			C config = null;

			try
			{
				bool isNeedToLoad = loadFromFile || loadOptions.HasFlag(LoadOptions.ForcedLoad);

				if (!isNeedToLoad)
					"Loading from config is DISABLED".logWarning();

				if (isNeedToLoad && File.Exists(configPath))
				{
					config = deserialize<C>(File.ReadAllText(configPath));
					config.configPath = configPath;
				}
				else
				{
					config = new C();
					config.save(configPath);
				}

				if (loadOptions.HasFlag(LoadOptions.SetAsMainConfig))
				{
					if (main == null)
						main = config;
					else
						"Config.main is already set".logWarning();
				}

				if (loadOptions.HasFlag(LoadOptions.ProcessAttributes))
					config.processAttributes();
			}
			catch (Exception e)
			{
				Log.msg(e);
			}

			return config;
		}

		public void save(string _configPath = null)
		{
			if (_configPath != null)
				configPath = _configPath;

			try
			{
				if (configPath != null)
					File.WriteAllText(configPath, serialize());
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}
	}
}