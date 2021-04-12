using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

#if GAME_SN && BRANCH_STABLE
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using Oculus.Newtonsoft.Json.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#endif

namespace Common.Configuration
{
	using Reflection;

	partial class Config
	{
		public partial class Field
		{
			// field with this attribute will not be saved (will be removed during save from json file)
			[AttributeUsage(AttributeTargets.Field)]
			public class LoadOnlyAttribute: Attribute {}
		}

		public class SerializerSettingsAttribute: Attribute
		{
			public bool verboseErrors = false;
			public bool ignoreNullValues = false;
			public bool ignoreDefaultValues = false;

			public Type[] converters = null;
		}

		class ConfigContractResolver: DefaultContractResolver
		{
			// serialize only fields (including private and readonly, except static and with NonSerialized attribute)
			// don't serialize properties
			protected override List<MemberInfo> GetSerializableMembers(Type objectType) =>
				objectType.fields().Where(field => !field.IsStatic && !field.checkAttr<NonSerializedAttribute>()).Cast<MemberInfo>().ToList();

			// we can deserialize all members and serialize members without LoadOnly attribute
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);

				property.Writable = true;
				property.Readable = !member.checkAttr<Field.LoadOnlyAttribute>();

				return property;
			}
		}


		static JsonSerializerSettings _initSerializer(Type configType)
		{
			var settings = new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented,
				ContractResolver = new ConfigContractResolver(),
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				Converters = { new StringEnumConverter() }
			};

			if (configType.getAttr<SerializerSettingsAttribute>() is SerializerSettingsAttribute settingsAttr)
			{
				if (settingsAttr.ignoreNullValues)	  settings.NullValueHandling = NullValueHandling.Ignore;
				if (settingsAttr.ignoreDefaultValues) settings.DefaultValueHandling = DefaultValueHandling.Ignore;

				if (settingsAttr.verboseErrors)
					settings.Error = (_, args) => $"<color=red>{args.ErrorContext.Error.Message}</color>".onScreen(); // TODO make more general

				if (settingsAttr.converters != null)
				{
					foreach (var type in settingsAttr.converters)
					{
						Debug.assert(typeof(JsonConverter).IsAssignableFrom(type));
						settings.Converters.Add(Activator.CreateInstance(type) as JsonConverter);
					}
				}
			}

			return settings;
		}

		JsonSerializerSettings srzSettings;

		string serialize() => JsonConvert.SerializeObject(this, srzSettings ??= _initSerializer(GetType()));

		static Config deserialize(string text, Type configType) => JsonConvert.DeserializeObject(text, configType, _initSerializer(configType)) as Config;
	}
}