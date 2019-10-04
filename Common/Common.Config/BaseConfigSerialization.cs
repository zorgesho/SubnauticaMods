using System.Reflection;

using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;

namespace Common.Config
{
	partial class Config
	{
		class ReadOnlyWritableContractResolver: DefaultContractResolver
		{
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);

				if (!property.Writable && member.MemberType == MemberTypes.Field && ((member as FieldInfo)?.IsInitOnly?? false))
					property.Writable = true;

				return property;
			}
		}
		
		static C deserialize<C>(string text)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings()
			{
				ContractResolver = new ReadOnlyWritableContractResolver()
			};
			
			return JsonConvert.DeserializeObject<C>(text, settings);
		}

		string serialize()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}
	}
}