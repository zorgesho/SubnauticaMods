using System;
using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		class KeyBindOption: ModOption
		{
			public KeyBindOption(InitParams p): base(p)	{}

			override public void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, (UnityEngine.KeyCode)fieldValue.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as KeybindChangedEventArgs)?.Key;
				base.onEvent(e);
			}
		}
	}
}