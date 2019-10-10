using Common;
using Common.Config;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051", Scope = "namespaceanddescendants", Target = "GravTrapImproved")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0060", Scope = "namespaceanddescendants", Target = "GravTrapImproved")]

namespace GravTrapImproved
{
	static public class Main
	{
		static internal ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyHelper.patchAll();

			Options.init();
		}
	}
}