using System.IO;
using System.Reflection;

namespace Common.PathHelpers
{
	static class ModPath
	{
		static public readonly string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar.ToString();

		static public string savesPath
		{
			get
			{
#if DEBUG && !GAME_SAVEPATH
				string path = Path.Combine(rootPath, "{dbgsave}");

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);														$"Using DEBUG SAVE PATH '{path}'".logWarning();

				return path;
#else
				return Path.Combine(SaveLoadManager.GetTemporarySavePath(), Strings.modName);
#endif
			}
		}
	}
}