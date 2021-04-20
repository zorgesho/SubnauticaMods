using System;

using UnityEngine;
using SMLHelper.V2.Options;

using Common;
using Common.Reflection;
using Common.Configuration;

#if GAME_SN
using UnityEngine.Events;
#endif

namespace CustomHotkeys
{
	using static SMLHelper.V2.Utility.KeyCodeUtils;
	using AddKeybindOption = Action<string, string, GameInput.Device, KeyCode>;
#if GAME_BZ
	using BindCallback = Action<GameInput.Device, GameInput.Button, GameInput.BindingSet, string>;
#endif

	class KeyWModBindOption: Options.ModOption
	{
		static MethodWrapper<AddKeybindOption> Options_AddKeybindOption = null;

#if GAME_BZ
		public class Tag: MonoBehaviour {}
#endif
		uGUI_Binding bind1, bind2;

		public KeyWModBindOption(Config.Field cfgField, string label): base(cfgField, label) {}

		public override void addOption(Options options)
		{
			if (!Options_AddKeybindOption) // HACK, using reflection to call protected method
			{
				var targetMethod = typeof(ModOptions).method("AddKeybindOption", typeof(string), typeof(string), typeof(GameInput.Device), typeof(KeyCode));
				Options_AddKeybindOption = targetMethod.wrap<AddKeybindOption>(options);
			}

			Options_AddKeybindOption.invoke(id, label, GameInput.Device.Keyboard, KeyCode.A);
		}

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
#if GAME_SN
			bind1.onValueChanged.RemoveAllListeners();
#endif
			GameObject bind2GO = UnityEngine.Object.Instantiate(bind1.gameObject);
			bind2GO.transform.SetParent(bindings.gameObject.transform, false);
			bind2 = bindings.bindings[1] = bind2GO.GetComponent<uGUI_Binding>();
#if GAME_BZ
			bind1.action = bind2.action = GameInput.Button.None;
#endif
			var keyValue = cfgField.value.cast<KeyWithModifier>();

			if (keyValue.modifier != KeyCode.None)
			{
				bind1.value = KeyCodeToString(keyValue.modifier);
				bind2.value = KeyCodeToString(keyValue.key);
			}
			else
			{
				bind1.value = keyValue.key != KeyCode.None? KeyCodeToString(keyValue.key): "";
				bind2.value = "";
			}
#if GAME_SN
			UnityAction<string> callback = new(_ => onValueChange());
			bind1.onValueChanged.AddListener(callback);
			bind2.onValueChanged.AddListener(callback);
#elif GAME_BZ
			bind1.gameObject.AddComponent<Tag>();
			bind2.gameObject.AddComponent<Tag>();

			BindCallback _getCallback(uGUI_Binding bind) => new((_, _, _, s) =>
			{
				bind.value = s;
				onValueChange();
				bind.RefreshValue();
			});

			bind1.bindCallback = _getCallback(bind1);
			bind2.bindCallback = _getCallback(bind2);
#endif
			base.onGameObjectChange(go);
		}
 	}
}