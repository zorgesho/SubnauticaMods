using System;
using System.IO;
using System.Reflection;

using Oculus.Newtonsoft.Json;

using Common.PathHelpers;

namespace Common.Config
{
	public class BaseConfig
	{
		[NonSerialized]
		static bool loadFromFile =
#if (DEBUG && !WRITE_CONFIG)
			false;
#else
			true;
#endif
		[NonSerialized]
		string configPath;

		// try to load config from mod folder. If file not found, create default config and save to that path
		static public C tryLoad<C>(string localPath = "config.json", bool processAttributes = true) where C: BaseConfig, new()
		{
			string configPath = ModPath.rootPath + localPath;
			C config = null;

			try
			{
				if (!loadFromFile)
					"Loading from config is DISABLED".logWarning();

				if (loadFromFile && File.Exists(configPath))
				{
					string configJson = File.ReadAllText(configPath);
					config = JsonConvert.DeserializeObject<C>(configJson);
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
				File.WriteAllText(configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}


		void processAttributes() => processAttributes(this); // using static method because of possible nested config classes
		
		static void processAttributes(object config)
		{
			if (config == null)
				return;

			// processing attributes for config class
			ConfigAttribute[] configAttrs = Attribute.GetCustomAttributes(config.GetType(), typeof(ConfigAttribute)) as ConfigAttribute[];

			foreach (var attr in configAttrs)
				attr.process(config);
			
			// processing attributes for fields and nested classes
			FieldInfo[] fields = config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

			foreach (FieldInfo field in fields)
			{																															$"Checking field '{field.Name}' for attributes".logDbg();
				if (Attribute.IsDefined(field, typeof(ConfigFieldAttribute)))
				{
					ConfigFieldAttribute[] attrs = Attribute.GetCustomAttributes(field, typeof(ConfigFieldAttribute)) as ConfigFieldAttribute[];

					foreach (var attr in attrs)
						attr.process(config, field);
				}

				if (field.FieldType.IsClass)
					processAttributes(field.GetValue(config));
			}
		}
	}
}