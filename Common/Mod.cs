using System;
using System.IO;
using System.Threading;
using System.Globalization;

namespace Common
{
	static partial class Mod
	{
		const string tmpFileName = "run the game to generate configs"; // name is also in the post-build.bat

		// supposed to be called before any other mod's code
		public static void init()
		{
			// may be overkill to make it for all mods and from the start
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try { File.Delete(Paths.modRootPath + tmpFileName); }
			catch (UnauthorizedAccessException) {}

			"Mod inited".logDbg();
		}
	}
}