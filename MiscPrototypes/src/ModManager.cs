using System.IO;
using System.Linq;
using System.Collections.Generic;

using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Linq;

using Common;
using Common.Configuration;

namespace MiscPrototypes
{
	static class ModManager
	{
		class ModConfig: Config
		{
			class ModSettingChanged: Field.IAction, IRootConfigInfo
			{
				ModConfig config;
				public void setRootConfig(Config config) => this.config = config as ModConfig;

				public void action() => config.updateSetting();
			}

			[Field.Action(typeof(ModSettingChanged))]
			bool enabled = true;

			void updateSetting()
			{
				modJson.Property("Enable").Value = enabled;
				File.WriteAllText(modJsonPath, JsonConvert.SerializeObject(modJson, Formatting.Indented));
			}

			JObject modJson;
			string  modJsonPath;

			public void init(string _modJsonPath)
			{
				modJsonPath = _modJsonPath;

				modJson = JsonConvert.DeserializeObject(File.ReadAllText(modJsonPath)) as JObject;
				Debug.assert(modJson != null);

				enabled = modJson.Property("Enable").Value.ToObject<bool>();

				var cfgField = new Field(this, nameof(enabled));
				var option = new Options.ToggleOption(cfgField, modJson.Property("DisplayName").Value.ToString());

				Options.add(option);
			}
		}

		static readonly List<ModConfig> modConfigs = new List<ModConfig>();

		static readonly string[] blacklist = new string[] { "MiscPrototypes", "Modding Helper" };

		public static void init()
		{
			string modsRootPath = Path.Combine(Paths.modRootPath, "..");

			foreach (var modPath in Directory.EnumerateDirectories(modsRootPath))
			{
				if (blacklist.FirstOrDefault(str => modPath.EndsWith(str)) != null)
					continue;

				ModConfig cfg = Config.tryLoad<ModConfig>(null, Config.LoadOptions.ProcessAttributes);
				cfg.init(Path.Combine(modPath, "mod.json"));

				modConfigs.Add(cfg);
			}
		}
	}
}