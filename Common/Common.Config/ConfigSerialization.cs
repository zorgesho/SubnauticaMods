using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;

namespace Common.Configuration
{
	partial class Config
	{
		class ConfigContractResolver: DefaultContractResolver
		{
			// serialize only fields (including private and readonly, except static and with NonSerialized attribute)
			// don't serialize properties
			protected override List<MemberInfo> GetSerializableMembers(Type objectType)
			{
				IEnumerable<MemberInfo> members = objectType.fields().Where(field => !field.IsStatic && !field.checkAttribute<NonSerializedAttribute>());
				return members.ToList();
			}

			// all serializable members are readable and writeble
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);

				property.Readable = property.Writable = true;

				return property;
			}
		}
		static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { ContractResolver = new ConfigContractResolver() };


		string serialize() => JsonConvert.SerializeObject(this, Formatting.Indented, serializerSettings);

		static C deserialize<C>(string text) => JsonConvert.DeserializeObject<C>(text, serializerSettings);
	}
}