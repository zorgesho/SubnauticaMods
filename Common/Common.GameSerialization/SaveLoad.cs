using System.IO;
using Oculus.Newtonsoft.Json;

namespace Common.GameSerialization
{
	static class SaveLoad
	{
		static public void save<T>(string id, T saveData)
		{
			File.WriteAllText(getPath(id), JsonConvert.SerializeObject(saveData, Formatting.None));
		}

		static public T load<T>(string id)
		{
			string filePath = getPath(id);

			return File.Exists(filePath)? JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath)): default;
		}

		static string getPath(string id) => Path.Combine(Paths.savesPath, id + ".json");
	}
}