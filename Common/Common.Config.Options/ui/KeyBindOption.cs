using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public class KeyBindOption: ModOption
		{
			public KeyBindOption(Config.Field cfgField, string label): base(cfgField, label) {}

			public override void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, (UnityEngine.KeyCode)cfgField.value.toInt());
			}

			public override void onEvent(EventArgs e)
			{
				cfgField.value = (e as KeybindChangedEventArgs)?.Key;
				base.onEvent(e);
			}
		}
	}
}