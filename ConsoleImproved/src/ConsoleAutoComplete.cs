using System.Collections.Generic;

using Common;

namespace ConsoleImproved
{
	static partial class ConsoleHelper
	{
		// used for entire console text, trying to complete last separate string
		static string tryCompleteText(string text)
		{
			int delimPos = text.LastIndexOf(' ');

			if (delimPos > 0) // trying complete parameter
			{
				string cmd = text.Substring(0, delimPos + 1).Trim();
				StringCache stringCache = techtypeCache; // default secondary cache

				if (cmd == "setcfgvar" || cmd == "getcfgvar") // hack, todo more general
					stringCache = cfgVarsCache;

				return cmd + " " + tryCompleteString(text.Substring(delimPos + 1), stringCache);
			}
			else
				return tryCompleteString(text, commandCache);
		}

		// used for last string from console text
		static string tryCompleteString(string str, StringCache stringCache)
		{
			var matched = stringCache.findByPrefix(str);

			if (matched.Count > 1)
				showMessages(matched, L10n.str("ids_matched"));

			return getCommonPrefix(matched, str);
		}


		static string getCommonPrefix(List<string> strings, string initialPrefix = "")
		{
			if (strings.Count == 0)
				return "";

			if (strings.Count == 1)
				return strings[0];

			string prefix = initialPrefix;
			bool res = true;
			int pos = prefix.Length, maxPos = strings[0].Length;

			while (res && pos < maxPos)
			{
				prefix += strings[0][pos++];				$"getCommonPrefix: testing {prefix}".logDbg();

				foreach (var s in strings)
				{
					if (!s.StartsWith(prefix))
					{
						res = false;
						break;
					}
				}
			}

			return res? prefix: prefix.Remove(prefix.Length - 1);
		}
	}
}