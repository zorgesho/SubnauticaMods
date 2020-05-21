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


		public class Hotkey
		{
			public string command;
			public string description;
			public KeyCode key;
		}

		public List<Hotkey> hotkeys = new List<Hotkey>()
		{
			new Hotkey { command = "setresolution 1280 720 true; setwindowpos 10 360", description = "Windowed", key = KeyCode.F1 },
			new Hotkey { command = "setresolution 2560 1440", description = "Fullscreen", key = KeyCode.F2 },
			new Hotkey { command = "item titanium 1; item gold 1; item gravsphere;", description = "Items", key = KeyCode.F5 }
		};
	}
}