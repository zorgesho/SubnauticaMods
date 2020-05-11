using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options
	{
		partial class Factory
		{
			class KeyBindOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type != typeof(UnityEngine.KeyCode))
						return null;

					return new KeyBindOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label);
				}
			}
		}


		public class KeyBindOption: ModOption
		{
			public KeyBindOption(Config.Field cfgField, string label): base(cfgField, label) {}

			public override void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, (UnityEngine.KeyCode)cfgField.value.toInt());
			}

			public override void onValueChange(EventArgs e)
			{
				cfgField.value = (e as KeybindChangedEventArgs)?.Key;
			}
		}
	}
}