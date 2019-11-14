using System;
using System.IO;
using System.Text;
using System.Reflection;

using UnityEngine;
using Harmony;
using Oculus.Newtonsoft.Json;

namespace FloatingCargoCrate
{
	public static class Main
	{
		public static Config config;

		public static void Patch()
		{
			HarmonyInstance.Create("FloatingCargoCrate").PatchAll(Assembly.GetExecutingAssembly());

			string configPath = Environment.CurrentDirectory + "\\QMods\\FloatingCargoCrate\\config.json";

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

			config.storageWidth = Math.Min(8, config.storageWidth);
			config.storageHeight = Math.Min(10, config.storageHeight);

			FloatingCargoCrateControl.massEmpty = config.crateMassEmpty;
			FloatingCargoCrateControl.massFull = config.crateMassFull;

			FloatingCargoCrate.PatchMe();
		}
	}
}