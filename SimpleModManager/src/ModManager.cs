using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Linq;

using Common;
using Common.Configuration;

namespace SimpleModManager
{
	static class ModManager
	{
		static readonly List<ModToggle> modToggles = new List<ModToggle>();

		class ModToggle: Config
		{
			[Field.Action(typeof(ModToggled))]
			bool enabled = true;

			class ModToggled: Field.IAction, IRootConfigInfo
			{
				ModToggle config;
				public void setRootConfig(Config config) => this.config = config as ModToggle;

				public void action() => config.updateMod();
			}

			void updateMod()
			{
				modJson.Property("Enable").Value = enabled;
				File.WriteAllText(modJsonPath, JsonConvert.SerializeObject(modJson, Formatting.Indented));
			}

			JObject modJson;
			string  modJsonPath;

			public bool init(string modJsonPath)
			{
				if (!File.Exists(modJsonPath))
					return false;

				this.modJsonPath = modJsonPath;

				try
				{
					modJson = JsonConvert.DeserializeObject(File.ReadAllText(modJsonPath)) as JObject;
					enabled = modJson.Property("Enable").Value.ToObject<bool>();

					var cfgField = new Field(this, nameof(enabled));
					var option = new Options.ToggleOption(cfgField, modJson.Property("DisplayName").Value.ToString());

					Options.add(option);

					return true;
				}
				catch (Exception e) { Log.msg(e); return false; }
			}
		}


		public static void init()
		{
			string modsRootPath = Path.Combine(Paths.modRootPath, "..");

			foreach (var modPath in Directory.EnumerateDirectories(modsRootPath))
			{
				if (Main.config.blacklist.FirstOrDefault(str => modPath.EndsWith(str)) != null)
					continue;

				var cfg = Config.tryLoad<ModToggle>(null, Config.LoadOptions.ProcessAttributes);

				if (cfg.init(Path.Combine(modPath, "mod.json")))
					modToggles.Add(cfg);
			}
		}
	}
}