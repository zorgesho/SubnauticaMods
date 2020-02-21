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

		protected virtual void onLoad() {} // called immediately after config loading/creating

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

				config = (!isNeedToLoad || !File.Exists(configPath))? new C(): deserialize<C>(File.ReadAllText(configPath));
				config.onLoad();

				// saving config even if we just loaded it to update it in case of added or removed fields (and to set configPath var)
				config.save(configPath);

				Debug.assert(!loadOptions.HasFlag(LoadOptions.SetAsMainConfig) || main == null, "Config.main is already set");
				if (loadOptions.HasFlag(LoadOptions.SetAsMainConfig) && main == null)
					main = config;

				if (loadOptions.HasFlag(LoadOptions.ProcessAttributes))
					config.processAttributes();
			}
			catch (Exception e) { Log.msg(e); }

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
			catch (Exception e) { Log.msg(e); }
		}
	}
}