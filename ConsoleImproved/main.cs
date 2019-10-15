using System.Diagnostics.CodeAnalysis;

using Common;
using Common.Config;

[assembly: SuppressMessage("Code Quality", "IDE0051", Scope = "namespaceanddescendants", Target = "ConsoleImproved")]
[assembly: SuppressMessage("Code Quality", "IDE0060", Scope = "namespaceanddescendants", Target = "ConsoleImproved")]


namespace ConsoleImproved
{
	static public class Main
	{
		static internal ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();
		}
	}
}