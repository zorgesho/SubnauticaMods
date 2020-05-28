using System;

using UnityEngine;
using UnityEngine.Events;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	using Reflection;
	using Object = UnityEngine.Object;
	using static SMLHelper.V2.Utility.KeyCodeUtils;

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

					if (cfgField.type == typeof(InputHelper.KeyWithModifier))
						return new KeyWModBindOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label);

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


		public class KeyWModBindOption: ModOption
		{
			uGUI_Binding bind1, bind2;

			public KeyWModBindOption(Config.Field cfgField, string label): base(cfgField, label) {}

			public override void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, KeyCode.None); // set actual value in onGameObjectChange
			}

			public override void onValueChange(EventArgs e) {}

			void onValueChange()
			{
				static KeyCode _getKeyCode(uGUI_Binding bind)
				{
					var keyCode = StringToKeyCode(bind.value);

					if (keyCode == KeyCode.AltGr)
					{
						keyCode = KeyCode.RightAlt;
						bind.value = keyCode.ToString(); // will resend event (field action will run once anyway)
					}

					return keyCode;
				}

				cfgField.value = new InputHelper.KeyWithModifier(_getKeyCode(bind1), _getKeyCode(bind2));
			}

			// uGUI_Binding derived from UnityEngine.UI.Selectable
			static readonly PropertyWrapper Component_gameObject =
				ReflectionHelper.safeGetType("UnityEngine.CoreModule", "UnityEngine.Component").property("gameObject").wrap();

			public override void onGameObjectChange(GameObject go)
			{
				uGUI_Bindings bindings = go.GetComponentInChildren<uGUI_Bindings>();
				bind1 = bindings.bindings[0];

				GameObject bind1GO = Component_gameObject.get<GameObject>(bind1);
				GameObject bind2GO = Object.Instantiate(bind1GO);
				bind2GO.transform.SetParent(bindings.gameObject.transform, false);
				bind2 = bindings.bindings[1] = bind2GO.GetComponent<uGUI_Binding>();

				var keyValue = cfgField.value.cast<InputHelper.KeyWithModifier>();

				if (keyValue.modifier != KeyCode.None)
				{
					bind1.value = KeyCodeToString(keyValue.modifier);
					bind2.value = KeyCodeToString(keyValue.key);
				}
				else
				{
					bind1.value = keyValue.key != KeyCode.None? KeyCodeToString(keyValue.key): "";
				}

				bind1.onValueChanged.RemoveAllListeners();

				var callback = new UnityAction<string>(_ => onValueChange());
				bind1.onValueChanged.AddListener(callback);
				bind2.onValueChanged.AddListener(callback);
			}
 		}
	}
}