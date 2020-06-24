using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using Oculus.Newtonsoft.Json.Serialization;

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


		static JsonSerializerSettings _initSerializer(Type configType)
		{
			var settings = new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented,
				ContractResolver = new ConfigContractResolver(),
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				Converters = new List<JsonConverter> { new StringEnumConverter(), new JsonConverters.KeyWithModifier() }
			};

			if (configType.getAttr<SerializerSettingsAttribute>() is SerializerSettingsAttribute settingsAttr)
			{
				if (settingsAttr.ignoreNullValues)	  settings.NullValueHandling = NullValueHandling.Ignore;
				if (settingsAttr.ignoreDefaultValues) settings.DefaultValueHandling = DefaultValueHandling.Ignore;

				if (settingsAttr.verboseErrors)
					settings.Error = (_, args) => $"<color=red>{args.ErrorContext.Error.Message}</color>".onScreen(); // TODO make more general
			}

			return settings;
		}

		JsonSerializerSettings srzSettings;

		string serialize() => JsonConvert.SerializeObject(this, srzSettings ??= _initSerializer(GetType()));

		static Config deserialize(string text, Type configType) => JsonConvert.DeserializeObject(text, configType, _initSerializer(configType)) as Config;


		class JsonConverters
		{
			public class KeyWithModifier: JsonConverter
			{
				public override bool CanConvert(Type objectType) =>
					objectType == typeof(InputHelper.KeyWithModifier);

				public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
					writer.WriteValue(value.ToString());

				public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
					(InputHelper.KeyWithModifier)(reader.Value as string);
			}
		}
	}
}