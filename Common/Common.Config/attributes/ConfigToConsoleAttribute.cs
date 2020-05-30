using System;
using System.Reflection;

namespace Common.Configuration
{
	using Utils;
	using Reflection;

	partial class Config
	{
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
		public class AddToConsoleAttribute: Attribute, IConfigAttribute, IFieldAttribute, IRootConfigInfo
		{
			[AttributeUsage(AttributeTargets.Field)]
			public class SkipAttribute: Attribute {} // don't add field to console

			readonly string varNamespace; // optional namespace for use in console in case of duplicate names
			readonly bool addPrivateFields, ignoreSkipAttr;

			Config rootConfig;
			public void setRootConfig(Config config) => rootConfig = config;

			public AddToConsoleAttribute(string varNamespace = null, bool addPrivateFields = false, bool ignoreSkipAttr = false)
			{
				this.varNamespace = varNamespace;
				this.addPrivateFields = addPrivateFields;
				this.ignoreSkipAttr = ignoreSkipAttr;
			}

			public void process(object config)
			{
				config.GetType().fields().forEach(field => process(config, field));
			}

			public void process(object config, FieldInfo field)
			{
				if (field.FieldType.IsPrimitive && (addPrivateFields || field.IsPublic) && (ignoreSkipAttr || !field.checkAttr<SkipAttribute>()))
					CfgVarBinder.addField(new FieldRanged(config, field, rootConfig), varNamespace);

				if (_isInnerFieldsProcessable(field))
					process(field.GetValue(config));
			}
		}
	}
}