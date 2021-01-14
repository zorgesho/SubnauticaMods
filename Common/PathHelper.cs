using System.IO;
using System.Reflection;

namespace Common
{
	static partial class Paths
	{
		public static readonly string modRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;

		// completes path with modRootPath if needed
		public static string makeRootPath(string filename)
		{
			return filename.isNullOrEmpty()? filename: ((Path.IsPathRooted(filename)? "": modRootPath) + filename);
		}

		// adds extension if it's absent
		public static string ensureExtension(string filename, string ext)
		{
			return filename.isNullOrEmpty()? filename: (Path.HasExtension(filename)? filename: filename + (ext.StartsWith(".")? "": ".") + ext);
		}

		// checks path for filename and creates intermediate directories if needed
		public static void ensurePath(string filename)
		{
			if (filename.isNullOrEmpty())
				return;

			string path = makeRootPath(Path.GetDirectoryName(filename));

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
		}
	}
}