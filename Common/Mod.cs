using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Globalization;

namespace Common
{
	static partial class Mod
	{
		const string tmpFileName = "run the game to generate configs"; // name is also in the post-build.bat

		static readonly Type qmmServices = ReflectionHelper.safeGetType("QModInstaller", "QModManager.API.QModServices");
		static readonly MethodInfo qmmServicesMain = qmmServices?.property("Main").GetGetMethod();
		static readonly MethodInfo qmmAddMessage = qmmServices?.method("AddCriticalMessage");

		// supposed to be called before any other mod's code
		public static void init()
		{
			// may be overkill to make it for all mods and from the start
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try { File.Delete(Paths.modRootPath + tmpFileName); }
			catch (UnauthorizedAccessException) {}

			"Mod inited".logDbg();
		}

		public static void addCriticalMessage(string msg, int size = MainMenuMessages.defaultSize, string color = MainMenuMessages.defaultColor)
		{
			if (qmmAddMessage != null)
				qmmAddMessage.Invoke(qmmServicesMain.Invoke(null, null), new object[] { msg, size, color, true });
			else
				MainMenuMessages.add(msg, size, color);
		}
	}
}