using System.IO;
using System.Reflection;

namespace Common.PathHelper
{
	static partial class Paths
	{
		static public readonly string modRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar.ToString();
	}
}