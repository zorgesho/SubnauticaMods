using System.IO;
using System.Reflection;

namespace Common
{
	static partial class Paths
	{
		public static readonly string modRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;
	}
}