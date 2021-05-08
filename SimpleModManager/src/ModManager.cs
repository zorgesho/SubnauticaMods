using System;
using System.IO;
using System.Collections.Generic;

#if GAME_SN && BRANCH_STABLE
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Linq;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

using Common;
using Common.Reflection;
using Common.Configuration;
using Common.Configuration.Actions;

namespace SimpleModManager
{
	static class ModManager
	{
		// used for console commands
		// we using list instead of dictionary because console commands can be used with partial mod name
		static readonly List<(string modName, Config.Field toggleField)> modToggleFields = new();

		class ConsoleCommands: PersistentConsoleCommands
		{
			public void mod_toggle(string modName) => setModEnabled(modName, null);

			public void mod_enable(string modName, bool enabled) => setModEnabled(modName, enabled);

			void setModEnabled(string modName, bool? enabled)
			{
				var mod = modToggleFields.Find(mod => mod.modName.Contains(modName));

				if (mod == default)
					return;

				bool enable = enabled ?? !mod.toggleField.value.cast<bool>();
				mod.toggleField.value = enable;
				$"{(enable? "<color=lime>enabled</color>": "<color=red>disabled</color>")}".onScreen(mod.modName);
			}
		}

		class ModToggle: Config
		{
			class HideHidden: Options.Components.Hider.IVisibilityChecker
			{ public bool visible => Main.config.showHiddenMods; }

			class HideBlacklisted: Options.Components.Hider.IVisibilityChecker
			{ public bool visible => Main.config.showBlacklistedMods; }

			[Field.Action(typeof(CallMethod), nameof(updateMod))]
			bool enabled = true;

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
					string modID = modJson.Property("Id").Value.ToString();

					string dir = Path.GetDirectoryName(modJsonPath);
					bool hidden = (new DirectoryInfo(dir).Attributes & FileAttributes.Hidden) != 0;
					bool blacklisted = Main.config.blacklist.contains(modID);

					Field cfgField = new (this, nameof(enabled));

					Options.nonUniqueOptionsIDsWarning = false;
					string optionName = $"{(hidden? "<color=silver>": "")}{modJson.Property("DisplayName").Value}{(hidden? "</color>": "")}";
					Options.ToggleOption option = new (cfgField, optionName);
					Options.nonUniqueOptionsIDsWarning = true;

					if (hidden)		 option.addHandler(new Options.Components.Hider.Add(new HideHidden(), "hidden-mod"));
					if (blacklisted) option.addHandler(new Options.Components.Hider.Add(new HideBlacklisted(), "blacklist-mod"));

					option.addHandler(new Options.Components.Tooltip.Add($"Version: {modJson.Property("Version").Value}"));
					option.addHandler(new Options.CustomOrderHandler(ModConfig.modIDBefore));

					Options.add(option);

					modToggleFields.Add((modID.ToLower(), cfgField));

					return true;
				}
				catch (Exception e) { Log.msg(e); return false; }
			}
		}


		public static void init()
		{
			PersistentConsoleCommands.register<ConsoleCommands>();

			foreach (var modPath in Directory.EnumerateDirectories(Path.Combine(Paths.modRootPath, "..")))
				Config.tryLoad<ModToggle>(null, Config.LoadOptions.ProcessAttributes).init(Path.Combine(modPath, "mod.json"));
		}
	}
}