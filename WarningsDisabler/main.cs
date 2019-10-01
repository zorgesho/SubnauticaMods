using System.Reflection;

using Harmony;

namespace WarningsDisabler
{
	//using static Common.Config.ConfigHelper;

	static public class Main
	{
		//static public Config config = tryLoadConfig<Config>();

		static public void patch()
		{
			HarmonyInstance.Create("WarningsDisabler").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}