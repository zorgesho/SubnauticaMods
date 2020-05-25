using System;
using System.IO;

namespace Common.Configuration
{
	abstract partial class Config
	{
		public const string defaultName = "config.json";

		public static Config main { get; private set; }

		[Flags]
		public enum LoadOptions
		{
			None = 0,
			ProcessAttributes = 1,
			MainConfig = 2,
			ForcedLoad = 4,
			ReadOnly = 8,
			Default = ProcessAttributes | MainConfig
		}

		static readonly bool ignoreExistingFile = // can be overrided by LoadOptions.ForcedLoad
#if (DEBUG && !LOAD_CONFIG)
			true;
#else
			false;
#endif
		public static string lastError { get; private set; }

		string configPath;

		protected virtual void onLoad() {} // called immediately after config loading/creating

		// try to load config from mod folder. If file not found, create default config and save it to that path
		public static C tryLoad<C>(string loadPath = defaultName, LoadOptions loadOptions = LoadOptions.Default) where C: Config
		{
			return tryLoad(typeof(C), loadPath, loadOptions) as C;
		}

		static Config tryLoad(Type configType, string loadPath, LoadOptions loadOptions)
		{
			Debug.assert(typeof(Config).IsAssignableFrom(configType), $"{configType}");

			Config config;
			string configPath = loadPath.isNullOrEmpty()? null: (Path.IsPathRooted(loadPath)? loadPath: Paths.modRootPath + loadPath);

			try
			{
				bool createDefault = (ignoreExistingFile && !loadOptions.HasFlag(LoadOptions.ForcedLoad)) || !File.Exists(configPath);

				if (createDefault && configPath != null)
					$"Creating default config ({loadPath})".log();

				config = createDefault? Activator.CreateInstance(configType) as Config: deserialize(File.ReadAllText(configPath), configType);
				config.onLoad();

				// saving config even if we just loaded it to update it in case of added or removed fields
				if (createDefault || !loadOptions.HasFlag(LoadOptions.ReadOnly))
					config.save(configPath);

				if (!loadOptions.HasFlag(LoadOptions.ReadOnly))
					config.configPath = configPath;

				if (loadOptions.HasFlag(LoadOptions.MainConfig))
				{																				"Config.main is already set!".logDbgError(main != null);
					main ??= config;
				}

				if (loadOptions.HasFlag(LoadOptions.ProcessAttributes))
					config.processAttributes();
			}
			catch (Exception e)
			{
				Log.msg(e, $"Exception while loading '{loadPath}'");
				lastError = e.Message;

				config = null;
			}

			return config;
		}

		public void save(string savePath = null)
		{
			string path = savePath ?? configPath;
			if (path == null)
				return;

			try { File.WriteAllText(path, serialize()); }
			catch (Exception e) { Log.msg(e, $"Exception while saving '{path}'"); }
		}
	}
}