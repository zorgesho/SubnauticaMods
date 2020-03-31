using System.Threading;
using System.Globalization;

namespace Common
{
	static partial class Mod
	{
		// supposed to be called before any other mod's code
		public static void init()
		{
			// may be overkill to make it for all mods and from the start
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			"Mod inited".logDbg();
		}
	}
}