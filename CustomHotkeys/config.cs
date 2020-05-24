using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

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

		class AddHotkeysAttribute: Attribute, IFieldAttribute
		{
			public void process(object config, FieldInfo field)
			{
				var hotkeys = field.GetValue(config) as List<Hotkey>;
				Common.Debug.assert(hotkeys != null);

				foreach (var hotkey in hotkeys)
				{
					if (hotkey.hide)
						continue;

					var cfgField = new Field(hotkey, nameof(Hotkey.key));
					var option = new Options.KeyWModBindOption(cfgField, hotkey.label ?? hotkey.command); // TODO clamp command and add tooltip with command

					Options.add(option);
				}
			}
		}

		public class Hotkey
		{
			class UpdateKeys: Field.IAction
			{ public void action() => HotkeyHelper.updateKeys(); }

			[Field.Action(typeof(UpdateKeys))]
			public InputHelper.KeyWithModifier key;

			public bool hide = false;
			public string command, label;
		}

		[AddHotkeys]
		public List<Hotkey> hotkeys = new List<Hotkey>()
		{
			new Hotkey { command = "setresolution 1280 720 true; setwindowpos 10 360 | setresolution 2560 1440", key = KeyCode.F1, label = "Toggle fullscreen" },
			new Hotkey { command = "pincfgvars all | pincfgvars", label = "Toggle cfgvars", key = KeyCode.F2 },
			new Hotkey { command = "warpforward 1", key = KeyCode.F4 },
			new Hotkey { command = "togglemod autoload", label = "Toggle AutoLoad", key = KeyCode.F11 },
			new Hotkey { command = "togglecfgvar misc.dbg.faststart.enabled", label = null /*"Toggle Fast Start"*/, key = KeyCode.F12 }
		};
	}
}