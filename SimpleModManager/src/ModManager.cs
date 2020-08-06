using System;
using System.IO;
using System.Collections.Generic;

#if BRANCH_EXP
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#else
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Linq;
#endif

using Common;
using Common.Reflection;
using Common.Configuration;

namespace SimpleModManager
{
	static class ModManager
	{
		static readonly List<Tuple<string, Config.Field>> modToggleFields = new List<Tuple<string, Config.Field>>();

		class ConsoleCommands: PersistentConsoleCommands
		{
			public void mod_toggle(string modName) => setModEnabled(modName, null);

			public void mod_enable(string modName, bool enabled) => setModEnabled(modName, enabled);

			void setModEnabled(string modName, bool? enabled)
			{
				var mod = modToggleFields.Find(mod => mod.Item1.Contains(modName));

				if (mod == null)
					return;

				bool enable = enabled ?? !mod.Item2.value.cast<bool>();
				mod.Item2.value = enable;
				$"{(enable? "<color=lime>enabled</color>": "<color=red>disabled</color>")}".onScreen(mod.Item1);
			}
		}

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

					Options.nonUniqueOptionsIDsWarning = false;
					var option = new Options.ToggleOption(cfgField, modJson.Property("DisplayName").Value.ToString());
					Options.nonUniqueOptionsIDsWarning = true;

					Options.add(option);

					modToggleFields.Add(Tuple.Create(modJson.Property("Id").Value.ToString().ToLower(), cfgField));

					return true;
				}
				catch (Exception e) { Log.msg(e); return false; }
			}
		}


		public static void init()
		{
			PersistentConsoleCommands.register<ConsoleCommands>();

			foreach (var modPath in Directory.EnumerateDirectories(Path.Combine(Paths.modRootPath, "..")))
			{
				if (Main.config.blacklist.findIndex(str => modPath.EndsWith(str)) == -1)
					Config.tryLoad<ModToggle>(null, Config.LoadOptions.ProcessAttributes).init(Path.Combine(modPath, "mod.json"));
			}
		}
	}
}