using System.IO;

using Oculus.Newtonsoft.Json;

namespace Common.GameSerialization
{
	static class SaveLoad
	{
		static public void save<T>(string id, T saveData)
		{
			string filePath = Path.Combine(PathHelper.Paths.savesPath, id + ".json");

			File.WriteAllText(filePath, JsonConvert.SerializeObject(saveData, Formatting.None));
		}

		static public T load<T>(string id)
		{
			string filePath = Path.Combine(PathHelper.Paths.savesPath, id + ".json");

			return File.Exists(filePath)? JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath)): default;
		}
	}
}