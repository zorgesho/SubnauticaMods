using System.IO;
using System.Reflection;

namespace Common.PathHelpers
{
	static class ModPath
	{
		static public readonly string rootPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + Path.DirectorySeparatorChar.ToString();
	}
}