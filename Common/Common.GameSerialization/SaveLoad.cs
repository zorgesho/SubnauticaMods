using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

#if BRANCH_EXP
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#else
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;
#endif

namespace Common.GameSerialization
{
	using Reflection;

	static class SaveLoad
	{
		class SaveContractResolver: DefaultContractResolver
		{
			// serialize only fields (including private and readonly, except static and with NonSerialized attribute)
			// don't serialize properties
			protected override List<MemberInfo> GetSerializableMembers(Type objectType) =>
				objectType.fields().Where(field => !field.IsStatic && !field.checkAttr<NonSerializedAttribute>()).ToList<MemberInfo>();

			// we can deserialize/serialize all members
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);
				property.Writable = property.Readable = true;

				return property;
			}
		}

		static readonly JsonSerializerSettings srzSettings = new JsonSerializerSettings()
		{
			Formatting = Mod.isDevBuild? Formatting.Indented: Formatting.None,
			ContractResolver = new SaveContractResolver()
		};

		public static void save<T>(string id, T saveData)
		{
			using var _ = Debug.profiler("SaveLoad.save");

			File.WriteAllText(getPath(id), JsonConvert.SerializeObject(saveData, srzSettings));
		}

		public static T load<T>(string id)
		{
			using var _ = Debug.profiler("SaveLoad.load");

			string filePath = getPath(id);
			return File.Exists(filePath)? JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), srzSettings): default;
		}

		public static bool load<T>(string id, out T saveData)
		{
			return !(saveData = load<T>(id)).Equals(default);
		}

		static string getPath(string id) => Path.Combine(Paths.savesPath, id + ".json");
	}
}