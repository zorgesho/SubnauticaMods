using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using Oculus.Newtonsoft.Json.Serialization;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class Field
		{
			// field with this attribute will not be saved (will be removed during save from json file)
			[AttributeUsage(AttributeTargets.Field)]
			public class LoadOnlyAttribute: Attribute {}
		}

		class ConfigContractResolver: DefaultContractResolver
		{
			// serialize only fields (including private and readonly, except static and with NonSerialized attribute)
			// don't serialize properties
			protected override List<MemberInfo> GetSerializableMembers(Type objectType)
			{
				IEnumerable<MemberInfo> members = objectType.fields().Where(field => !field.IsStatic && !field.checkAttr<NonSerializedAttribute>());
				return members.ToList();
			}

			// we can deserialize all members and serialize members without LoadOnly attribute
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);

				property.Writable = true;
				property.Readable = !member.checkAttr<Field.LoadOnlyAttribute>();

				return property;
			}
		}
		static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
		{
			ContractResolver = new ConfigContractResolver(),
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};


		string serialize() => JsonConvert.SerializeObject(this, Formatting.Indented, serializerSettings);

		static C deserialize<C>(string text) => JsonConvert.DeserializeObject<C>(text, serializerSettings);
	}
}