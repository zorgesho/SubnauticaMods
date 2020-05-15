using System.Reflection;

using Harmony;
using UnityEngine;

namespace Common
{
	static partial class Debug
	{
		/// messages with the same prefix will stay in the same message slot (also <see cref="StringExtensions.onScreen(string, string)"/>)
		public static void addMessage(string message, string prefix)
		{
			PersistentScreenMessages.patch();
			ErrorMessage.AddDebug($"[{prefix}] {message}");
		}

		static class PersistentScreenMessages
		{
			static bool patched = false;
			static readonly FieldInfo messageEntry = typeof(ErrorMessage._Message).field("entry");
			static readonly ReflectionHelper.PropertyWrapper text = ReflectionHelper.safeGetType("UnityEngine.UI", "UnityEngine.UI.Text").propertyWrap("text");

			public static void patch()
			{
				if (patched || !(patched = true))
					return;

				if (!HarmonyHelper.isPatchedBy(typeof(ErrorMessage).method("_AddMessage"), nameof(messagePatch))) // patching this only once
					HarmonyHelper.patch();
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(ErrorMessage), "_AddMessage")]
			static bool messagePatch(ErrorMessage __instance, string messageText)
			{
				if (messageText.isNullOrEmpty() || messageText[0] != '[')
					return true;

				int prefixEnd = messageText.IndexOf(']');

				if (prefixEnd > 0)
				{
					string prefix = messageText.Substring(0, prefixEnd + 1);
					var msg = __instance.messages.Find(m => m.messageText.StartsWith(prefix));

					if (msg != null)
					{
						msg.timeEnd = Time.time + __instance.timeFadeOut + __instance.timeDelay;
						text.set(messageEntry.GetValue(msg), messageText);

						return false;
					}
				}

				return true;
			}
		}
	}
}