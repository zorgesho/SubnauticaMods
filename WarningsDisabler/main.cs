using System.Reflection;

using Harmony;
using Common.Config;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051", Scope = "namespaceanddescendants", Target = "WarningsDisabler")]

namespace WarningsDisabler
{
	static public class Main
	{
		static internal readonly ModConfig config = Config.tryLoad<ModConfig>();

		static public void patch()
		{
			HarmonyInstance.Create("WarningsDisabler").PatchAll(Assembly.GetExecutingAssembly());

			Options.init();
		}
	}
}