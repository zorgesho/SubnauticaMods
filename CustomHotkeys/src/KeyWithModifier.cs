using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

using Common;
using Common.Reflection;

#if GAME_SN && BRANCH_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace CustomHotkeys
{
	struct KeyWithModifier
	{
		public readonly KeyCode modifier, key;

		public KeyWithModifier(KeyCode key1, KeyCode key2 = KeyCode.None)
		{
			if (key1 == KeyCode.None || key2 == KeyCode.None) // if only one key defined treat it like a normal key
			{
				modifier = KeyCode.None;
				key = key1 == KeyCode.None? key2: key1;
				return;
			}

			bool isKey1Mod = isModifier(key1);
			bool isKey2Mod = isModifier(key2);

			if (isKey1Mod && !isKey2Mod)
			{
				modifier = key1;
				key = key2;
			}
			else if (!isKey1Mod && isKey2Mod)
			{
				modifier = key2;
				key = key1;
			}
			else // if both keys are modifiers or non-modifiers then use only first key
			{
				modifier = KeyCode.None;
				key = key1;
			}
		}

		static readonly HashSet<KeyCode> _modifiers = new()
		{
			KeyCode.LeftAlt, KeyCode.RightAlt,
			KeyCode.LeftShift, KeyCode.RightShift,
			KeyCode.LeftControl, KeyCode.RightControl
		};

		public static bool isModifier(KeyCode keyCode) => _modifiers.Contains(keyCode);
		public static ReadOnlyCollection<KeyCode> modifiers => _modifiers.ToList().AsReadOnly();

		public static implicit operator KeyWithModifier(KeyCode keyCode) => new (keyCode);

		public static bool operator ==(KeyWithModifier key1, KeyWithModifier key2) => key1.key == key2.key && key1.modifier == key2.modifier;
		public static bool operator !=(KeyWithModifier key1, KeyWithModifier key2) => !(key1 == key2);

		public override int  GetHashCode() => ((int)modifier & 0xFFF) << 12 | ((int)key & 0xFFF);
		public override bool Equals(object obj) => obj is KeyWithModifier key && this == key;

		public override string ToString() => this == default? "": (modifier == KeyCode.None? $"{key}": $"{modifier}+{key}");

		public static explicit operator KeyWithModifier(string str)
		{
			if (str.isNullOrEmpty())
				return default;

			try
			{
				var keys = str.Split('+');
				return new KeyWithModifier(keys[0].convert<KeyCode>(), keys.Length == 2? keys[1].convert<KeyCode>(): KeyCode.None);
			}
			catch (Exception e) { Log.msg(e); return default; }
		}
	}

	class KWM_JsonConverter: JsonConverter
	{
		public override bool CanConvert(Type objectType) =>
			objectType == typeof(KeyWithModifier);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
			writer.WriteValue(value.ToString());

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
			(KeyWithModifier)(reader.Value as string);
	}
}