using System.IO;
using Oculus.Newtonsoft.Json;

namespace Common.GameSerialization
{
	static class SaveLoad
	{
		public static void save<T>(string id, T saveData)
		{
			File.WriteAllText(getPath(id), JsonConvert.SerializeObject(saveData, Formatting.None));
		}

		public static T load<T>(string id)
		{
			string filePath = getPath(id);
			return File.Exists(filePath)? JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath)): default;
		}

		public static bool load<T>(string id, out T saveData) => !(saveData = load<T>(id)).Equals(default);

		static string getPath(string id) => Path.Combine(Paths.savesPath, id + ".json");
	}
}