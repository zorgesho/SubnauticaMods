using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
	static partial class StringExtensions
	{
		public static string onScreen(this string s)
		{
			ErrorMessage.AddDebug(s);
			return s;
		}
		
		public static void onScreen(this List<string> list, string msg = "", int maxCount = 30)
		{
			List<string> listToPrint = list.Count > maxCount? list.GetRange(0, maxCount): list;

			if (list.Count > maxCount)
				$"List is too large ({list.Count} entries), printing first {maxCount} entries".onScreen();

			listToPrint.ForEach(s => ErrorMessage.AddDebug(msg + s));
		}
	}

	static class Strings
	{
		public static class Mouse
		{
			public static readonly string middleButton	= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57405) + "</color>";
			public static readonly string scrollUp		= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57406) + "</color>";
			public static readonly string scrollDown	= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57407) + "</color>";
		}
		
		public static readonly string modName = Assembly.GetExecutingAssembly().GetName().Name;
	}


	// base class for console commands which are exists between scenes
	abstract class PersistentConsoleCommands: MonoBehaviour
	{
		const string cmdPrefix = "OnConsoleCommand_";

		readonly List<string> cmdNames = new List<string>();

		public static GameObject createGameObject<T>(string name = "ConsoleCommands") where T: PersistentConsoleCommands
		{
			GameObject obj = new GameObject(name, typeof(T), typeof(SceneCleanerPreserve));
			DontDestroyOnLoad(obj);

			return obj;
		}

		void init()
		{																													"PersistentConsoleCommands.init cmdNames already inited!".logDbgError(cmdNames.Count > 0);
			// searching for console commands methods in derived class
			MethodInfo[] methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			methods.Where(m => m.Name.StartsWith(cmdPrefix)).forEach(m => cmdNames.Add(m.Name.Replace(cmdPrefix, "")));
		}
			
		void registerCommands()
		{
			foreach (var cmdName in cmdNames)
			{
				// double registration is checked inside DevConsole
				DevConsole.RegisterConsoleCommand(this, cmdName);															$"PersistentConsoleCommands: {cmdName} is registered".logDbg();
			}
		}
			
		void Awake()
		{
			init();
			SceneManager.sceneUnloaded += onSceneUnloaded;

			registerCommands();
		}

		void OnDestroy()
		{
			SceneManager.sceneUnloaded -= onSceneUnloaded;
		}

		// notifications are cleared between some scenes, so we need to reregister commands
		void onSceneUnloaded(Scene scene)
		{
			registerCommands();
		}
	}
}