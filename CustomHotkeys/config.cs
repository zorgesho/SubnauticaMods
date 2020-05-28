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


		[NonSerialized, NoInnerFieldsAttrProcessing]
		static readonly List<Options.ModOption> bindOptions = new List<Options.ModOption>();

		static void addBindOptions(List<Hotkey> hotkeys)
		{
			Common.Debug.assert(hotkeys != null);

			bindOptions.ForEach(option => Options.remove(option));
			bindOptions.Clear();

			try
			{
				int id = 0;
				foreach (var hotkey in hotkeys)
				{
					if (hotkey.hide)
						continue;

					var cfgField = new Field(hotkey, nameof(Hotkey.key), Main.config, $"hotkey.{id++:D2}");
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

			public bool up, held;
			public string command;

			public bool hide;
			public string label;
#pragma warning restore
		}

		class HotkeyListChanged: Field.IAction
		{
			public void action()
			{
				addBindOptions(Main.config.hotkeys);
				Options.resetPanel();
			}
		}

		class AddHotkeysAttribute: Attribute, IFieldAttribute
		{
			public void process(object config, FieldInfo field) =>
				addBindOptions(field.GetValue(config) as List<Hotkey>);
		}

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