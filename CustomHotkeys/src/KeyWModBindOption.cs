using System;

using UnityEngine;
using UnityEngine.Events;

using Common;
using Common.Reflection;
using Common.Configuration;

namespace CustomHotkeys
{
	using static SMLHelper.V2.Utility.KeyCodeUtils;

	class KeyWModBindOption: Options.KeyBindOption
	{
		public class Creator: Options.ICreator
		{
			public Options.ModOption create(Config.Field cfgField)
			{
				if (cfgField.type == typeof(KeyWithModifier))
					return new KeyWModBindOption(cfgField, cfgField.getAttr<Options.FieldAttribute>()?.label);

				return null;
			}
		}

		uGUI_Binding bind1, bind2;

		public KeyWModBindOption(Config.Field cfgField, string label): base(cfgField, label) {}

		public override void onValueChange(EventArgs e) {}

		void onValueChange()
		{
			static KeyCode _getKeyCode(uGUI_Binding bind)
			{
				if (bind.value.isNullOrEmpty())
					return default;

				var keyCode = StringToKeyCode(bind.value);

				if (keyCode == KeyCode.AltGr)
				{
					keyCode = KeyCode.RightAlt;
					bind.value = keyCode.ToString(); // will resend event (field action will run once anyway)
				}
				else if (keyCode == KeyCode.None)
				{
					bind.value = ""; // in case of unsupported binds (e.g. mouse wheel)
				}

				return keyCode;
			}

			cfgField.value = new KeyWithModifier(_getKeyCode(bind1), _getKeyCode(bind2));
		}

		public override void onGameObjectChange(GameObject go)
		{
			uGUI_Bindings bindings = go.GetComponentInChildren<uGUI_Bindings>();
			bind1 = bindings.bindings[0];
			bind1.onValueChanged.RemoveAllListeners();

			GameObject bind2GO = UnityEngine.Object.Instantiate(bind1.gameObject);
			bind2GO.transform.SetParent(bindings.gameObject.transform, false);
			bind2 = bindings.bindings[1] = bind2GO.GetComponent<uGUI_Binding>();

			var keyValue = cfgField.value.cast<KeyWithModifier>();

			if (keyValue.modifier != KeyCode.None)
			{
				bind1.value = KeyCodeToString(keyValue.modifier);
				bind2.value = KeyCodeToString(keyValue.key);
			}
			else
			{
				bind1.value = keyValue.key != KeyCode.None? KeyCodeToString(keyValue.key): "";
			}

			var callback = new UnityAction<string>(_ => onValueChange());
			bind1.onValueChanged.AddListener(callback);
			bind2.onValueChanged.AddListener(callback);

			base.onGameObjectChange(go);
		}
 	}
}