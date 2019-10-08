using System.IO;

namespace Common.PathHelper
{
	static partial class Paths
	{
		static public string savesPath
		{
			get
			{
#if DEBUG && !GAME_SAVEPATH
				string path = Path.Combine(modRootPath, "{dbgsave}");

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