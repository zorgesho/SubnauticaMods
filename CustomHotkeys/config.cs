//#define GENERATE_SAMPLE_CONFIG

using System;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Configuration;
using Common.Configuration.Actions;

namespace CustomHotkeys
{
	class ModConfig: Config
	{
		[Options.Field("Enable developer tools hotkeys", "Use <b>F1</b>, <b>F3</b> and <b>F6</b> for vanilla developer tools (you can reassign those tools to the other hotkeys)")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool enableDevToolsHotkeys = !Mod.Consts.isDevBuild;

		class OnlyInMainMenu: Options.Components.Hider.IVisibilityChecker
		{ public bool visible => Options.mode == Options.Mode.MainMenu; }

		[Options.Field("Enable feedback collector", "Use <b>F8</b> to show feedback collector")]
		[Options.Hideable(typeof(OnlyInMainMenu))]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		[Options.FinalizeAction(typeof(FeedbackCollectorPatch.SettingChanged))]
		public readonly bool enableFeedback = !Mod.Consts.isDevBuild;

		public class HotkeyHider: Field.IAction
		{ public void action() => Options.Components.Hider.refresh("hotkeys"); }

		[Options.Field("Show unassigned hotkeys")]
		[Field.Action(typeof(HotkeyHider))]
		public readonly bool showUnassigned = true;

		[Options.Field("Show hidden hotkeys", "Show hotkeys which are set to hidden in <b>" + Main.hotkeyConfigName + "</b>")]
		[Field.Action(typeof(HotkeyHider))]
		public readonly bool showHidden = false;

#if GAME_SN // BZ is using right click for that
		public readonly bool easyBindRemove = true;
#endif
		public readonly bool addConsoleCommands = true;

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


	[SerializerSettings(ignoreNullValues = true, verboseErrors = true, converters = new[] { typeof(KWM_JsonConverter) })]
	class HKConfig: Config
	{
		[NonSerialized, NoInnerFieldsAttrProcessing]
		readonly List<Options.ModOption> bindOptions = new();

		class HotkeyHider: Options.Components.Hider.IVisibilityChecker
		{
			readonly Hotkey hotkey;
			public HotkeyHider(Hotkey hotkey) => this.hotkey = hotkey;

			public bool visible => (Main.config.showUnassigned || hotkey.key != default) &&
								   (Main.config.showHidden || hotkey.hidden != true);
		}

		public void addHotkey(Hotkey hotkey)
		{
			hotkeys.Add(hotkey);
			save();

			refreshHotkeyList();
		}

		void refreshHotkeyList(bool resetOptionsPanel = true)
		{
			addBindOptions();

			if (resetOptionsPanel)
				Options.Utils.resetPanel();
		}

		void addBindOptions()
		{
			Common.Debug.assert(hotkeys != null);

			bindOptions.ForEach(Options.remove);
			bindOptions.Clear();

			try
			{
				int id = 0;
				foreach (var hotkey in hotkeys)
				{
					bool hidden = hotkey.hidden == true;
					var cfgField = new Field(hotkey, nameof(Hotkey.key), this, $"hotkey.{id++:D2}");
					var label = !string.IsNullOrWhiteSpace(hotkey.label)? hotkey.label: hotkey.command.clampLength(30).Replace("...", "<color=silver>...</color>");

					if (hidden)
						label = $"<color=silver>{label}</color>";

					var option = new KeyWModBindOption(cfgField, label);

					var tooltip = "<color=white><b>Command: </b></color>";
					tooltip += hotkey.command.Replace(";", "<color=orange><b>;</b></color>").Replace("|", "<color=yellow><b>|</b></color>");
					option.addHandler(new Options.Components.Tooltip.Add(tooltip, false));
					option.addHandler(new Options.Components.Hider.Add(new HotkeyHider(hotkey), "hotkeys"));

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
			public enum Mode { Press = 0, PressRelease = 1, Hold = 2 }

			class UpdateBinds: Field.IAction
			{ public void action() => HotkeyHelper.updateBinds(); }

			[Field.Action(typeof(UpdateBinds))]
			[Field.Action(typeof(ModConfig.HotkeyHider))]
			public KeyWithModifier key;

			public string command;
			public Mode? mode; // nullable so it can be ignored by serializer

			public string label;
			public bool? hidden; // ^
#pragma warning restore
		}

		class AddHotkeysAttribute: Attribute, IRootConfigInfo
		{ public void setRootConfig(Config config) => (config as HKConfig).refreshHotkeyList(false); }

		[Field.Action(typeof(CallMethod), nameof(refreshHotkeyList), true)]
		[AddHotkeys, Field.Reloadable, NoInnerFieldsAttrProcessing]
		public readonly List<Hotkey> hotkeys = new()
		{
#if DEBUG && !GENERATE_SAMPLE_CONFIG
			new Hotkey { command = "autoforward", label = "Autoforward", key = KeyCode.LeftAlt },
			new Hotkey { command = "setresolution 1280 720 false; setwindowpos 10 10 | setresolution 2560 1440", key = KeyCode.F1, label = "Toggle fullscreen" },
			new Hotkey { command = "spawn seamoth; warpforward 10; speed 10; vehicle_enter; wait 2; speed 1; clearmessages", key = KeyCode.None, label = "Spawn seamoth" },
			new Hotkey { command = "showmodoptions", label = "Open mod options", key = KeyCode.F3 },
			new Hotkey { command = "pincfgvars all | pincfgvars", label = "Toggle cfgvars", key = KeyCode.F2 },

			new Hotkey { command = "freecam", label = "Free cam", key = KeyCode.F5 },
			new Hotkey { command = "devtools_toggleframegraph", label = "Toggle frame graph", key = KeyCode.F7 },
			new Hotkey { command = "devtools_wireframe", label = "Toggle wireframe", key = KeyCode.F8 },
			new Hotkey { command = "game_startnew", label = "Start new game", key = KeyCode.F11 },
			new Hotkey { command = "togglecfgvar misc.dbg.faststart.enabled", label = "Toggle Fast Start", key = KeyCode.F12 },
			new Hotkey { command = "devtools_hidegui mask; fov 5 | fov 60; devtools_hidegui none", label = "Zoom in", key = KeyCode.V, mode = Hotkey.Mode.PressRelease },
			new Hotkey { command = "warpforward 1", key = new KeyWithModifier(KeyCode.W, KeyCode.LeftAlt), mode = Hotkey.Mode.Hold, label = "Warp forward" },
#else
			new Hotkey { command = "autoforward", label = "Autoforward", key = KeyCode.LeftAlt },
			new Hotkey { command = "useitem firstaidkit", label = "Use medkit", key = KeyCode.H },
			new Hotkey { command = "vehicle_enter", label = "Enter nearby vehicle", key = KeyCode.E },
			new Hotkey { command = "showmodoptions", label = "Open mod options", key = new KeyWithModifier(KeyCode.O, KeyCode.RightAlt) },
#endif

#if GENERATE_SAMPLE_CONFIG
			new Hotkey { command = "setresolution 1280 720 false; setwindowpos 10 10 | setresolution 2560 1440", key = KeyCode.F2, label = "Toggle fullscreen" },
			new Hotkey { command = "bindslot 0 flashlight; equipslot 0", key = KeyCode.None, label = "Equip flashlight" },
			new Hotkey { command = "bindslot 1 propulsioncannon; equipslot 1 | bindslot 1 repulsioncannon; equipslot 1", key = KeyCode.None, label = "Switch cannons" },
			new Hotkey { command = "bindslot 0 seaglide; equipslot 0; autoforward true | bindslot 0 flashlight; equipslot 0; autoforward false", key = KeyCode.LeftControl, label = "Toggle seaglide" },
			new Hotkey { command = "useitem filteredwater disinfectedwater bigfilteredwater stillsuitwater", key = KeyCode.None, label = "Drink water" },
			new Hotkey { command = "lastcommand", key = KeyCode.L, label = "Run last console command" },
			new Hotkey { command = "warpforward 1", key = KeyCode.UpArrow, mode = Hotkey.Mode.Hold, label = "Warp forward" },
			new Hotkey { command = "spawn seamoth; warpforward 10; speed 10; vehicle_enter; wait 2; speed 1; clearmessages", key = KeyCode.None, label = "Spawn seamoth" },
			new Hotkey { command = "devtools_hidegui mask; fov 5 | fov 60; devtools_hidegui none", key = KeyCode.V, mode = Hotkey.Mode.PressRelease, label = "Zoom in" },
#endif
		};
	}
}