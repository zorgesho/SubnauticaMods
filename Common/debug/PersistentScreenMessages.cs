using System.Reflection;

using Harmony;
using UnityEngine;

namespace Common
{
	static partial class Debug
	{
		/// messages with the same prefix will stay in the same message slot (usage: <see cref="StringExtensions.onScreen(string, string)"/>)
		[HarmonyPatch(typeof(ErrorMessage), "_AddMessage")]
		static class PersistentScreenMessages
		{
			static readonly FieldInfo messageEntry = typeof(ErrorMessage._Message).field("entry");
			static readonly ReflectionHelper.PropertyWrapper text = ReflectionHelper.safeGetType("UnityEngine.UI", "UnityEngine.UI.Text").propertyWrap("text");

			// patching this only once (don't check for actual patch method for now)
			static bool Prepare()
			{
#if TRACE
				var patches = HarmonyHelper.getPatchInfo(typeof(ErrorMessage).method("_AddMessage"));						"ErrorMessage.AddMessage is already patched, skipping".logDbg(patches != null);
				return patches == null;
#else
				return false;
#endif
			}

			static bool Prefix(ErrorMessage __instance, string messageText)
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