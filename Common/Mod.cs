using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Globalization;

namespace Common
{
	using static ReflectionHelper;

	static partial class Mod
	{
		const string tmpFileName = "run the game to generate configs"; // name is also in the post-build.bat

		static readonly Type qmmServices = safeGetType("QModInstaller", "QModManager.API.QModServices");
		static readonly PropertyWrapper qmmServicesMain = qmmServices.propertyWrap("Main");
		static readonly MethodWrapper qmmAddMessage = qmmServices.methodWrap("AddCriticalMessage");

		static readonly PropertyWrapper qmmQModDisplayName = safeGetType("QModInstaller", "QModManager.API.IQMod").propertyWrap("DisplayName");

		public static readonly string id = Assembly.GetExecutingAssembly().GetName().Name;
		public static readonly string name = qmmQModDisplayName.get<string>(qmmServices.methodWrap("GetMyMod").invoke(qmmServicesMain.get()));

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
			if (qmmAddMessage)
				qmmAddMessage.invoke(qmmServicesMain.get(), msg, size, color, true);
			else
				MainMenuMessages.add(msg, size, color);
		}
	}
}