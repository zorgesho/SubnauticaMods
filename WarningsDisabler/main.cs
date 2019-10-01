using System.Reflection;

using Harmony;
using Common.Config;

namespace WarningsDisabler
{
	static public class Main
	{
		static public Config config = BaseConfig.tryLoad<Config>();

		static public void patch()
		{
			HarmonyInstance.Create("WarningsDisabler").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}