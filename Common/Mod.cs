﻿using System;
using System.IO;
using System.Threading;
using System.Globalization;

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
		const string tmpFileName = "run the game to generate configs"; // name is also in the post-build.bat

		public static string id   { get { init(); return _id; } }
		public static string name { get { init(); return _name; } }
		static string _id, _name;

		static bool inited;

		// supposed to be called before any other mod's code
		public static void init()
		{
			if (inited || !(inited = true))
				return;

			// may be overkill to make it for all mods and from the start
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try { File.Delete(Paths.modRootPath + tmpFileName); }
			catch (UnauthorizedAccessException) {}

			var manifest = SimpleJSON.Parse(File.ReadAllText(Paths.modRootPath + "mod.json"));

			_id = manifest["Id"];
			_name = manifest["DisplayName"];

			if (manifest["UpdateCheck"].AsBool)
			{
				var currentVersion = new Version(manifest["Version"]);
				var latestVersion = VersionChecker.getLatestVersion(manifest["VersionURL"]);							$"Latest version is {latestVersion}".logDbg();

				if (latestVersion > currentVersion)
					addCriticalMessage($"UPDATE: current: {currentVersion} latest: {latestVersion}", color: "yellow"); // TODO message
			}

			"Mod inited".logDbg();
		}

		static readonly Type qmmServices = ReflectionHelper.safeGetType("QModInstaller", "QModManager.API.QModServices");
		static readonly PropertyWrapper qmmServicesMain = qmmServices.property("Main").wrap();
		static readonly MethodWrapper qmmAddMessage = qmmServices.method("AddCriticalMessage").wrap();

		public static void addCriticalMessage(string msg, int size = MainMenuMessages.defaultSize, string color = MainMenuMessages.defaultColor)
		{
			if (qmmAddMessage)
				qmmAddMessage.invoke(qmmServicesMain.get(), msg, size, color, true);
			else
				MainMenuMessages.add(msg, size, color);
		}
	}
}