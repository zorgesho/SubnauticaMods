using System;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace CustomHotkeys
{
	class ModConfig: Config
	{
		[Options.Field]
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

		[Options.Field]
		[Options.FinalizeAction(typeof(FeedbackEnabler))]
		public readonly bool enableFeedback = !Mod.isDevBuild;


		public class Switch
		{
			[NonSerialized]
			public int index = 0;

			public string id;
			public string[] commands;
		}

		public List<Switch> switches = new List<Switch>()
		{
			new Switch { id = "switch_test", commands = new string[] { "msg switch1", "msg switch2"} },
			new Switch { id = "toggle_res", commands = new string[] { "setresolution 1280 720 true; setwindowpos 10 360", "setresolution 2560 1440"} },
			new Switch { id = "switch_cfgvars", commands = new string[] { "pincfgvars all", "pincfgvars"} },
			new Switch { id = "switch_speed", commands = new string[] { "speed 10", "speed 1"} },
		};


		public class Hotkey
		{
			public string command;
			public string description;
			public KeyCode key;
		}

		public List<Hotkey> hotkeys = new List<Hotkey>()
		{
			new Hotkey { command = "toggle_res", description = "Toggle fullscreen", key = KeyCode.F1 },
			new Hotkey { command = "switch_cfgvars", description = "Toggle cfgvars", key = KeyCode.F2 },
			new Hotkey { command = "togglecfgvar misc.dbg.faststart.enabled", description = "Toggle Fast Start", key = KeyCode.F12 }
		};
	}
}