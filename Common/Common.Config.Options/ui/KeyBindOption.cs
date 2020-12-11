using System;

using UnityEngine;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	using Reflection;

	partial class Options
	{
		partial class Factory
		{
			class KeyBindOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type == typeof(KeyCode))
						return new KeyBindOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label);

					return null;
				}
			}
		}


		public class KeyBindOption: ModOption
		{
			public KeyBindOption(Config.Field cfgField, string label): base(cfgField, label) {}

			public override void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, cfgField.value.convert<KeyCode>());
			}

			public override void onValueChange(EventArgs e)
			{
				cfgField.value = (e as KeybindChangedEventArgs)?.Key;
			}
 		}
	}
}