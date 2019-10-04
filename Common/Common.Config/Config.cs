using System;
using System.IO;

using Common.PathHelpers;

namespace Common.Config
{
	partial class Config
	{
		static bool loadFromFile =
#if (DEBUG && !WRITE_CONFIG)
			false;
#else
			true;
#endif
		string configPath;

		// try to load config from mod folder. If file not found, create default config and save to that path
		static public C tryLoad<C>(string localPath = "config.json", bool processAttributes = true) where C: Config, new()
		{
			string configPath = ModPath.rootPath + localPath;
			C config = null;

			try
			{
				if (!loadFromFile)
					"Loading from config is DISABLED".logWarning();

				if (loadFromFile && File.Exists(configPath))
				{
					config = deserialize<C>(File.ReadAllText(configPath));
					config.configPath = configPath;
				}
				else
				{
					config = new C();
					config.save(configPath);
				}

				if (processAttributes)
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
				File.WriteAllText(configPath, serialize());
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}
	}
}