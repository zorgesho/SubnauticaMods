using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public class KeyBindOption: ModOption
		{
			public KeyBindOption(Config.Field cfgField, string label, string tooltip = null): base(cfgField, label, tooltip) {}

			public override void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, (UnityEngine.KeyCode)cfgField.value.toInt());
			}

			public override void onChangeValue(EventArgs e)
			{
				cfgField.value = (e as KeybindChangedEventArgs)?.Key;
			}
		}
	}
}