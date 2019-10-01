using System.IO;
using System.Reflection;

namespace Common.PathHelpers
{
	class ModPath
	{
		static public readonly string rootPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + Path.DirectorySeparatorChar.ToString();

		string path;

		public ModPath(string _path) => path = rootPath + _path;

		static public implicit operator string(ModPath p) => p.path;
		static public explicit operator ModPath(string s) => new ModPath(s);
	}
}