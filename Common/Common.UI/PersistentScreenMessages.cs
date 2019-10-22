using Harmony;

namespace Common
{
	static partial class MiscGameExtensions
	{
		// messages with the same prefix will stay in the one message slot
		static public string onScreen(this string s, string prefix)
		{
			ErrorMessage.AddDebug($"[{prefix}] {s}");
			return s;
		}
	}
	
	namespace UI
	{
		[HarmonyPatch(typeof(ErrorMessage), "_AddMessage")]
		static class ErrorMessage__AddMessage_Patch
		{
			static bool Prefix(ErrorMessage __instance, string messageText)
			{
				if (messageText != null && messageText.Length > 0 && messageText[0] == '[')
				{
					int prefixEnd = messageText.IndexOf(']');

					if (prefixEnd > 0)
					{
						string prefix = messageText.Substring(0, prefixEnd + 1);
						var msg = __instance.messages.Find(m => m.messageText.StartsWith(prefix));

						if (msg != null)
						{
							msg.timeToDelete = UnityEngine.Time.time + __instance.fadeTime + __instance.fadeDelay;
							msg.entry.text = messageText;

							return false;
						}
					}
				}

				return true;
			}
		}
	}
}