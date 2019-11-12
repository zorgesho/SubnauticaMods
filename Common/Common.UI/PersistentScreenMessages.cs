using System.Reflection;
using System.Diagnostics;

using Harmony;

namespace Common
{
	static partial class StringExtensions
	{
		// messages with the same prefix will stay in the one message slot
		[Conditional("TRACE")]
		public static void onScreen(this string s, string prefix) => ErrorMessage.AddDebug($"[{prefix}] {s}");
	}


	namespace UI
	{
		[HarmonyPatch(typeof(ErrorMessage), "_AddMessage")]
		static class ErrorMessage_AddMessage_Patch
		{
			// patching this only once (hoping that no one is also patching this)
			static bool Prepare()
			{
#if TRACE
				MethodInfo method = typeof(ErrorMessage).GetMethod("_AddMessage", BindingFlags.NonPublic | BindingFlags.Instance);
				Patches patches = HarmonyHelper.harmonyInstance.GetPatchInfo(method);
																													"ErrorMessage.AddMessage is already patched!".logDbg(patches != null);
				return patches == null;
#else
				return false;
#endif
			}

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
							msg.timeEnd = UnityEngine.Time.time + __instance.timeFadeOut + __instance.timeDelay;
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