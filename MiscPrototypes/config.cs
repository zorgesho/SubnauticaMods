//#define TEST_NESTED_CLASSES
//#define TEST_MULTIPLE_CONFIGS
//#define TEST_OPTIONS_ADJUST

using System;
using System.Reflection;

using Common;
using Common.Configuration;

namespace MiscPrototypes
{
#if TEST_MULTIPLE_CONFIGS
	public static partial class Main
	{
		internal static readonly ModConfig  config1_double = Config.tryLoad<ModConfig>("config_double.json");
		internal static readonly ModConfig2 config2 = Config.tryLoad<ModConfig2>("config2.json");
	}

	[Options.Field]
	[AddToConsole("test_all_2")]
	class ModConfig2: Config
	{
		public readonly bool field1_config2 = true;

		[Field.Range(-50f, 50f)]
		public readonly float field2_config2 = 0f;
	}
#endif

#if TEST_NESTED_CLASSES || TEST_MULTIPLE_CONFIGS
	[Options.Field]
	[AddToConsole("test_all")]
#endif
	class ModConfig: Config
	{
#if TEST_MULTIPLE_CONFIGS
		public readonly bool field1_config1 = true;
#endif

#if TEST_NESTED_CLASSES
		class TestNestedAttribute: Attribute, IFieldAttribute, IRootConfigInfo
		{
			Config rootConfig;
			public void setRootConfig(Config config) => rootConfig = config;

			public void process(object config, FieldInfo field)
			{
				string path = null;

				using (Debug.profiler($"getFieldPath:{field.Name}"))
					path = new Field(config, field, rootConfig).path;

				$"----- field: '{field.Name}' fieldPath: '{path}'".logDbg();
			}
		}


		[TestNested]
		public readonly bool field1 = true;

		public class SimpleNestedClass
		{
			[TestNested]
			public readonly bool nestedField = true;

			public readonly float floatNestedField = 0f;
		}
		public readonly SimpleNestedClass nestedClass = new SimpleNestedClass();


		public class TestDuplicates
		{
			[TestNested]
			[AddToConsole("dup")]
			public readonly bool duplicate = true;

			public readonly float floatDuplicate = 0f;
		}

		public readonly TestDuplicates duplicateClass1 = new TestDuplicates();
		public readonly TestDuplicates duplicateClass2 = new TestDuplicates();
		public readonly TestDuplicates duplicateClass3 = new TestDuplicates();


		public class DeepNestClass
		{
			public class NestedClass1
			{
				[TestNested]
				public readonly bool nestedField1 = true;

				public readonly float floatNestedField1 = 0f;

				public class NestedClass2
				{
					[TestNested]
					public readonly bool nestedField2 = true;

					[Field.Range(-100f, 100f)]
					public readonly float floatNestedField2 = 0f;
				}
				public readonly NestedClass2 nestedClass2 = new NestedClass2();
			}
			public readonly NestedClass1 nestedClass1 = new NestedClass1();
		}
		[AddToConsole("deep")]
		public readonly DeepNestClass deepNestClass = new DeepNestClass();
#endif

#if TEST_OPTIONS_ADJUST
		[Options.Field("Short description of choice")]
		[Options.Choice("Choice 1", "Choice 2", "Choice 3")]
		public readonly int choice1 = 0;

		[Options.Field("Long string with description of this choice")]
		[Options.Choice("Choice 1", "Choice 2", "Choice 3")]
		public readonly int choice2 = 0;

		[Options.Field("Short description of slider")]
		[Field.Range(0, 1000)]
		public readonly float slider1 = 500f;

		[Options.Field("Long string with description of this slider")]
		[Field.Range(0, 100)]
		public readonly float slider2 = 50f;

		[Options.Field("Short description of checkbox")]
		public readonly bool toggle1 = false;

		[Options.Field("Really long string with description of this checkbox")]
		public readonly bool toggle2 = true;

		[Options.Field("Key description")]
		public readonly UnityEngine.KeyCode keybind1 = UnityEngine.KeyCode.A;

		[Options.Field("Long string with description of this key")]
		public readonly UnityEngine.KeyCode keybind2 = UnityEngine.KeyCode.B;
#endif
	}
}