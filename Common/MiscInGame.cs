using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
	using Reflection;

	static partial class StringExtensions
	{
		public static string onScreen(this string s)
		{
			if (!GameUtils.isLoadingState && Time.timeScale != 0f)
				ErrorMessage.AddDebug(s);

			return s;
		}

		// messages with the same prefix will stay in the same message slot
		public static string onScreen(this string s, string prefix) { Debug.addMessage(s, prefix); return s; }

		public static void onScreen(this List<string> list, string msg = "", int maxCount = 30)
		{
			var listToPrint = list.Count > maxCount? list.GetRange(0, maxCount): list;
			listToPrint.ForEach(s => ErrorMessage.AddDebug(msg + s));
		}

		public static string onScreen(this List<string> list, string prefix)
		{
			var sb = new StringBuilder();
			list.ForEach(line => sb.AppendLine(line));
			return sb.ToString().onScreen(prefix);
		}
	}

	static class Strings
	{
		public static class Mouse
		{
			static string _str(int utf32) => "<color=#ADF8FFFF>" + char.ConvertFromUtf32(utf32) + "</color>";

			public static readonly string rightButton	= _str(57404);
			public static readonly string middleButton	= _str(57405);
			public static readonly string scrollUp		= _str(57406);
			public static readonly string scrollDown	= _str(57407);
		}
	}

	static partial class SpriteHelper // extended in other Common projects
	{
		public static Atlas.Sprite getSprite(object spriteID)
		{
			$"TechSpriteHelper.getSprite({spriteID.GetType()}) is not implemented!".logError();
			return SpriteManager.defaultSprite;
		}
	}

	static class GameUtils
	{
		// can't use vanilla GetVehicle in OnPlayerModeChange after 06.11 update :(
		public static Vehicle getVehicle(this Player player) => player.GetComponentInParent<Vehicle>();

		public static TechType getHeldToolType() => Inventory.main?.GetHeldTool()?.pickupable.GetTechType() ?? TechType.None;

		public static bool isLoadingState => uGUI._main?.loading.loadingBackground.state == true;

		public static void clearScreenMessages() => // expire all messages except QMM main menu messages
			ErrorMessage.main?.messages.Where(m => m.timeEnd - Time.time < 1e3f).forEach(m => m.timeEnd = Time.time - 1f);
	}

	static class MiscInGameExtensions
	{
		public static int getArgCount(this NotificationCenter.Notification n) => n?.data?.Count ?? 0;

		public static string getArg(this NotificationCenter.Notification n, int index) => _getArg(n, index) as string;

		public static T getArg<T>(this NotificationCenter.Notification n, int index) => _getArg(n, index).convert<T>();
		public static T getArg<T>(this NotificationCenter.Notification n, int index, T defaultValue) =>
			index < n.getArgCount()? n.getArg<T>(index): defaultValue;

		static object _getArg(this NotificationCenter.Notification n, int index) => n?.data?.Count > index? n.data[index]: null;
	}


	// base class for console commands which are exists between scenes
	abstract class PersistentConsoleCommands: MonoBehaviour
	{
		protected class CommandDataAttribute: Attribute
		{
			public bool caseSensitive = false;
			public bool combineArgs = false;
		}

		const string cmdPrefix = "OnConsoleCommand_";

		List<Tuple<string, CommandDataAttribute>> commands;

		public static GameObject createGameObject<T>(string name = "ConsoleCommands") where T: PersistentConsoleCommands
		{
			return UnityHelper.createPersistentGameObject<T>(name);
		}

		void registerCommands()
		{
			// searching for console commands methods in derived class
			commands ??= GetType().methods().Where(m => m.Name.StartsWith(cmdPrefix)).
											 Select(m => Tuple.Create(m.Name.Replace(cmdPrefix, ""), m.getAttr<CommandDataAttribute>())).
											 ToList();

			foreach (var command in commands)
			{
				bool caseSensitive = command.Item2?.caseSensitive ?? false;
				bool combineArgs = command.Item2?.combineArgs ?? false;

				// double registration is checked inside DevConsole
				DevConsole.RegisterConsoleCommand(this, command.Item1, caseSensitive, combineArgs);						$"PersistentConsoleCommands: {command.Item1} is registered".logDbg();
			}
		}

		void Awake()
		{
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