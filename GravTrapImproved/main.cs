using System;
using System.IO;
using System.Reflection;

using Harmony;
using Oculus.Newtonsoft.Json;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051", Scope = "namespaceanddescendants", Target = "GravTrapImproved")]

namespace GravTrapImproved
{
	static public class Main
	{
		static internal Config config;

		static public void patch()
		{
			HarmonyInstance.Create("GravTrapImproved").PatchAll(Assembly.GetExecutingAssembly());

			string configPath = Environment.CurrentDirectory + "\\QMods\\GravTrapImproved\\config.json";

			if (File.Exists(configPath))
			{
				string configJson = File.ReadAllText(configPath);
				config = JsonConvert.DeserializeObject<Config>(configJson);
			}
			else
			{
				config = new Config();
				File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
			}
		}
	}
}