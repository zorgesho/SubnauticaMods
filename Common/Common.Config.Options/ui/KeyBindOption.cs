using System;
using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		class KeyBindOption: ModOption
		{
			public KeyBindOption(Config.CfgField cfgField, string label): base(cfgField, label) {}

			override public void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, (UnityEngine.KeyCode)cfgField.value.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				cfgField.value = (e as KeybindChangedEventArgs)?.Key;
				base.onEvent(e);
			}
		}
	}
}