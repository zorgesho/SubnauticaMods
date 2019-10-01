using System;
using System.IO;
using System.Reflection;

using Oculus.Newtonsoft.Json;

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

		static public C tryLoad<C>(string localPath = "config.json", bool processAttributes = true) where C: BaseConfig, new()
		{
			string configPath = PathHelpers.ModPath.rootPath + localPath;
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


		void processAttributes() => processAttributes(this); // using static method because of possible nested config classes
		
		static void processAttributes(object config)
		{
			if (config == null)
				return;

			BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			FieldInfo[] fields = config.GetType().GetFields(bf);

			foreach (FieldInfo field in fields)
			{																															$"Checking field '{field.Name}' for attributes".logDbg();
				if (Attribute.IsDefined(field, typeof(ConfigFieldAttribute)))
				{
					ConfigFieldAttribute configField = (ConfigFieldAttribute)Attribute.GetCustomAttribute(field, typeof(ConfigFieldAttribute));
					configField.validate(config, field);
				}

				if (field.FieldType.IsClass)
					processAttributes(field.GetValue(config));
			}
		}

		public void save(string _configPath = "")
		{
			if (_configPath != "")
				configPath = _configPath;

			try
			{
				File.WriteAllText(configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
			}
			catch (System.Exception e)
			{
				Log.msg(e);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	class ConfigFieldAttribute: Attribute
	{
		public float min = float.MinValue;
		public float max = float.MaxValue;

		public void validate(object config, FieldInfo field)
		{																						$"ConfigFieldAttribute.validate min > max, field '{field.Name}'".logDbgError(min > max);
			try
			{
				float value = Convert.ToSingle(field.GetValue(config));
				float valuePrev = value;

				value = UnityEngine.Mathf.Clamp(value, min, max);

				if (value != valuePrev)
				{																				$"ConfigFieldAttribute.validate changing field '{field.Name}' from {valuePrev} to {value}".logWarning();
					field.SetValue(config, Convert.ChangeType(value, field.FieldType));
				}
			}
			catch (Exception e)
			{
				Log.msg(e, $"config field {field.Name}");
			}
		}
	}
}