using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
	static class MiscGameExtensions
	{
		static public string onScreen(this string s)
		{
			ErrorMessage.AddDebug(s);
			return s;
		}
	}

	static class Strings
	{
		static public class Mouse
		{
			static public readonly string middleButton	= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57405) + "</color>";
			static public readonly string scrollUp		= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57406) + "</color>";
			static public readonly string scrollDown	= "<color=#ADF8FFFF>" + char.ConvertFromUtf32(57407) + "</color>";
		}
		
		static public readonly string modName = Assembly.GetExecutingAssembly().GetName().Name;
	}

	
	// base class for console commands which are exists between scenes
	abstract class PersistentConsoleCommands: MonoBehaviour
	{
		const string cmdPrefix = "OnConsoleCommand_";

		readonly List<string> cmdNames = new List<string>();

		static public GameObject createGameObject<T>(string name = "ConsoleCommands") where T: PersistentConsoleCommands
		{
			GameObject obj = new GameObject(name, typeof(T), typeof(SceneCleanerPreserve));
			DontDestroyOnLoad(obj);

			return obj;
		}

		void init()
		{																													"PersistentConsoleCommands.init cmdNames already inited!".logDbgError(cmdNames.Count > 0);
			// searching for console commands methods in derived class
			MethodInfo[] methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var m in methods)
				if (m.Name.StartsWith(cmdPrefix))
					cmdNames.Add(m.Name.Replace(cmdPrefix, string.Empty));
		}
			
		void registerCommands(bool checkNeedReregister = false)
		{
			foreach (var cmdName in cmdNames)
			{
				if (!checkNeedReregister || NotificationCenter.DefaultCenter.notifications[cmdPrefix + cmdName] == null)
				{																											$"PersistentConsoleCommands: {cmdName} is registered".logDbg();
					DevConsole.RegisterConsoleCommand(this, cmdName);
				}
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

		// notifications are cleared between some scenes, so we need to reregister command
		void onSceneUnloaded(Scene scene)
		{
			registerCommands(true);
		}
	}
}