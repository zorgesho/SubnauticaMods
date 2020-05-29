using System;
using System.Reflection;
using System.Collections.Generic;

#if DEBUG
using UnityEngine;
#endif

using Common;
using Common.Harmony;
using Common.Configuration;

namespace CustomHotkeys
{
	class ModConfig: Config
	{
		[Options.Field] // TODO label & tooltip
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool enableDevToolsHotkeys = !Mod.isDevBuild;

		class FeedbackEnabler: Field.IAction
		{
			public void action()
			{
				if (uGUI_FeedbackCollector.main)
					uGUI_FeedbackCollector.main.enabled = Main.config.enableFeedback;
			}
		}

		[Options.Field] // TODO label & tooltip
		[Options.FinalizeAction(typeof(FeedbackEnabler))]
		public readonly bool enableFeedback = !Mod.isDevBuild;

		public readonly bool easyBindRemove = true;

		class OpenConfig: Field.IAction
		{
			public void action()
			{
				string executable = WinApi.getExecutableByExtension(".json") ?? WinApi.getExecutableByExtension(".txt");
				string configPath = Paths.modRootPath + Main.hotkeyConfigName;
				WinApi.startProcess(executable ?? configPath, configPath);
			}
		}

		[Field.Action(typeof(OpenConfig))]
		[Options.Button, Options.Field("Open <b>" + Main.hotkeyConfigName + "</b>")]
#pragma warning disable CS0169, IDE0044
		int _;
#pragma warning restore
	}


	[SerializerSettings(ignoreNullValues = true)]
	class HKConfig: Config
	{
		[NonSerialized, NoInnerFieldsAttrProcessing]
		readonly List<Options.ModOption> bindOptions = new List<Options.ModOption>();

		void addBindOptions()
		{
			Common.Debug.assert(hotkeys != null);

			bindOptions.ForEach(option => Options.remove(option));
			bindOptions.Clear();

			try
			{
				int id = 0;
				foreach (var hotkey in hotkeys)
				{
					if (hotkey.hide == true)
						continue;

					var cfgField = new Field(hotkey, nameof(Hotkey.key), this, $"hotkey.{id++:D2}");
					var option = new Options.KeyWModBindOption(cfgField, hotkey.label ?? hotkey.command); // TODO clamp command and add tooltip with command

					bindOptions.Add(option);
					Options.add(option);
				}
			}
			catch (Exception e) { Log.msg(e); }

			HotkeyHelper.setKeys(hotkeys);
		}

		public class Hotkey
		{
#pragma warning disable CS0649 // field is never assigned to
			class UpdateBinds: Field.IAction
			{ public void action() => HotkeyHelper.updateBinds(); }

			[Field.Action(typeof(UpdateBinds))]
			public InputHelper.KeyWithModifier key;

			public string command;
			public bool? up, held; // nullables so they can be ignored by serializer

			public string label;
			public bool? hide; // ^
#pragma warning restore
		}

		class HotkeyListChanged: Field.IAction, IRootConfigInfo
		{
			HKConfig hkConfig;
			public void setRootConfig(Config config) => hkConfig = config as HKConfig;

			public void action()
			{
				hkConfig.addBindOptions();
				Options.resetPanel();
			}
		}

		class AddHotkeysAttribute: Attribute, IRootConfigInfo
		{ public void setRootConfig(Config config) => (config as HKConfig).addBindOptions(); }

		[Field.Action(typeof(HotkeyListChanged))]
		[AddHotkeys, Field.Reloadable, NoInnerFieldsAttrProcessing]
		public List<Hotkey> hotkeys = new List<Hotkey>()
		{
#if DEBUG
			new Hotkey { command = "setresolution 1280 720 true; setwindowpos 10 360 | setresolution 2560 1440", key = KeyCode.F1, label = "Toggle fullscreen" },
			new Hotkey { command = "pincfgvars all | pincfgvars", label = "Toggle cfgvars", key = KeyCode.F2 },
			new Hotkey { command = "warpforward 1", key = KeyCode.F4, held = true },
			new Hotkey { command = "togglemod autoload", label = "Toggle AutoLoad", key = KeyCode.F11 },
			new Hotkey { command = "togglecfgvar misc.dbg.faststart.enabled", label = null /*"Toggle Fast Start"*/, key = KeyCode.F12 }
#endif
		};
	}
}