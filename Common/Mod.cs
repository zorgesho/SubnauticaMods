#define DISABLE_VERSION_CHECK_IN_DEVBUILD

using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Globalization;

using UnityEngine;

namespace Common
{
	using Utils;
	using Reflection;

	static partial class Mod
	{
		public const bool isDevBuild =
#if DEBUG
			true;
#else
			false;
#endif
		public const bool isBranchStable =
#if BRANCH_STABLE
			true;
#elif BRANCH_EXP
			false;
#endif

		public static bool isShuttingDown { get; private set; }
		class ShutdownListener: MonoBehaviour { void OnApplicationQuit() { isShuttingDown = true; "Shutting down".logDbg(); } }

		const string tmpFileName = "run the game to generate configs"; // name is also in the post-build.bat
		const string updateMessage = "An update is available! (current version is v<color=orange>{0}</color>, new version is v<color=orange>{1}</color>)";

		public static readonly string id = Assembly.GetExecutingAssembly().GetName().Name; // not using mod.json for ID
		public static string name { get { init(); return _name; } }
		static string _name;

		static bool inited;

		// supposed to be called before any other mod's code
		public static void init()
		{
			if (inited || !(inited = true))
				return;

			UnityHelper.createPersistentGameObject<ShutdownListener>($"{id}.ShutdownListener");

			// may be overkill to make it for all mods and from the start
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try { File.Delete(Paths.modRootPath + tmpFileName); }
			catch (UnauthorizedAccessException) {}

			var manifest = SimpleJSON.Parse(File.ReadAllText(Paths.modRootPath + "mod.json"));
			_name = manifest["DisplayName"];
			bool needCheckVer = manifest["UpdateCheck"].AsBool;

#if DISABLE_VERSION_CHECK_IN_DEVBUILD
			if (needCheckVer && isDevBuild)
				"Version check is disabled for dev build!".logDbg();

			needCheckVer &= !isDevBuild;
#endif
			if (needCheckVer)
			{
				var currentVersion = new Version(manifest["Version"]);
				var latestVersion = VersionChecker.getLatestVersion(manifest["VersionURL"]);							$"Latest version is {latestVersion}".logDbg();

				if (latestVersion > currentVersion)
					addCriticalMessage(updateMessage.format(currentVersion, latestVersion), color: "yellow");
			}

			"Mod inited".logDbg();
		}

		static readonly Type qmmServices = Type.GetType("QModManager.API.QModServices, QModInstaller");
		static readonly PropertyWrapper qmmServicesMain = qmmServices.property("Main").wrap();
		static readonly MethodWrapper qmmAddMessage = qmmServices.method("AddCriticalMessage").wrap();

		public static void addCriticalMessage(string msg, int size = MainMenuMessages.defaultSize, string color = MainMenuMessages.defaultColor)
		{
			if (qmmAddMessage)
				qmmAddMessage.invoke(qmmServicesMain.get(), msg, size, color, true);
			else
				MainMenuMessages.add(msg, size, color);
		}

		static readonly MethodWrapper qmmGetMod = qmmServices.method("GetMod", typeof(string)).wrap();

		public static bool isModEnabled(string modID)
		{
			return qmmGetMod.invoke(qmmServicesMain.get(), modID)?.getPropertyValue<bool>("Enable") == true;
		}
	}
}