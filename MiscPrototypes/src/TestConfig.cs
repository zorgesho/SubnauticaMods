#region Tests settings
//#define TEST_NESTED_CLASSES
//#define TEST_MULTIPLE_CONFIGS
//#define TEST_LANGUAGE_HELPER

//#define CONFIG_FORCE_LOAD

//options stuff
//#define TEST_CHOICES
//#define TEST_OPTIONS_ADJUST
//#define TEST_TOOLTIPS
//#define TEST_CUSTOM_FORMATS
//#define TEST_CUSTOM_VALUE
//#define TEST_SLIDERS_RANGE
//#define TEST_ACTIONS
#endregion

using System;
using System.Threading;
using System.Reflection;

using Common;
using Common.Configuration;

namespace MiscPrototypes
{
	public static partial class Main
	{
		internal static TestConfig testConfig;

		// called after Mod.init
		static partial void initTestConfig()
		{
			testConfig = Config.tryLoad<TestConfig>("test_config.json",
#if CONFIG_FORCE_LOAD
				Config.LoadOptions.ForcedLoad |
#endif
				Config.LoadOptions.ProcessAttributes);
		}
	}

#if TEST_MULTIPLE_CONFIGS
	public static partial class Main
	{
		internal static readonly TestConfig  testConfig_double = Config.tryLoad<TestConfig>("test_config_double.json");
		internal static readonly TestConfig2 testConfig2 = Config.tryLoad<TestConfig2>("test_config2.json");
	}

	[Options.Field]
	[AddToConsole("test_all_2")]
	class TestConfig2: Config
	{
		public readonly bool field1_config2 = true;

		[Field.Range(-50f, 50f)]
		public readonly float field2_config2 = 0f;
	}
#endif

#if TEST_LANGUAGE_HELPER
	class L10n: LanguageHelper
	{
		public static string testString01 = "TEST01";
		public static string testString02 = "TEST02";
		public static string testString03 = "TEST03";
		public static string testString04 = "TEST04";
		public static string testString05 = "TEST05";
		public static string testString06 = "TEST06";
		public static string testString07 = "TEST07";
		public static string testString08 = "TEST08";
		public static string testString09 = "TEST09";
		public static string testString10 = "TEST10";

		public static string testString11 = "TEST11";
		public static string testString12 = "TEST12";
		public static string testString13 = "TEST13";
		public static string testString14 = "TEST14";
		public static string testString15 = "TEST15";
		public static string testString16 = "TEST16";
		public static string testString17 = "TEST17";
		public static string testString18 = "TEST18";
		public static string testString19 = "TEST19";
		public static string testString20 = "TEST20";
	}
#endif

#if TEST_NESTED_CLASSES || TEST_MULTIPLE_CONFIGS
	[Options.Field]
	[AddToConsole("test_all")]
#endif
	class TestConfig: Config
	{
#if TEST_ACTIONS
		[AddToConsole]
		public class ActionsTest
		{
			class SimpleAction1: Field.IAction
			{
				public void action() => "SIMPLE ACTION 1".onScreen();
			}

			class SimpleAction2: Field.IAction
			{
				public void action() => "SIMPLE ACTION 2".onScreen();
			}

			class ComplexAction1: Field.IAction
			{
				public void action()
				{
					"COMPLEX ACTION 1".onScreen();
					Thread.Sleep(500);
				}
			}

			[Options.Field]
			[Field.Action(typeof(SimpleAction1))]
			public readonly bool toggle1_Field_SimpleAction1 = false;

			[Options.Field]
			[Field.Action(typeof(SimpleAction2))]
			public readonly bool toggle1_Field_SimpleAction2 = false;

			[Options.Field]
			[Field.Range(0, 100)]
			[Field.Action(typeof(SimpleAction1))]
			public readonly float slider1_Field_SimpleAction1 = 1;

			[Options.Field]
			[Field.Range(0, 100)]
			[Field.Action(typeof(ComplexAction1))]
			public readonly float slider1_Field_ComplexAction1 = 1;

			[Options.Field]
			[Field.Range(0, 100)]
			[Options.FinalizeAction(typeof(SimpleAction1))]
			public readonly float slider1_SimpleAction1 = 1;

			[Options.Field]
			[Field.Range(0, 100)]
			[Options.FinalizeAction(typeof(ComplexAction1))]
			public readonly float slider1_ComplexAction1 = 1;

			[Options.Field]
			[Options.FinalizeAction(typeof(SimpleAction1))]
			public readonly bool toggle1_SimpleAction1 = false;

			[Options.Field]
			[Options.FinalizeAction(typeof(SimpleAction1))]
			public readonly bool toggle2_SimpleAction1 = false;

			[Options.Field]
			[Options.FinalizeAction(typeof(SimpleAction2))]
			public readonly bool toggle1_SimpleAction2 = false;

			[Options.Field]
			[Options.FinalizeAction(typeof(SimpleAction2))]
			public readonly bool toggle2_SimpleAction2 = false;
		}
		public readonly ActionsTest actions = new ActionsTest();
#endif

#if TEST_TOOLTIPS
		[Options.Field("Toggle with tooltip", "Toggle tooltip")]
		public readonly bool tooltipToggle1 = true;

		[Options.Field("Key bind with tooltip", "Key bind tooltip")]
		public readonly UnityEngine.KeyCode tooltipKeybind1 = UnityEngine.KeyCode.A;

		[Options.Field("Choice with tooltip", "Choice tooltip")]
		[Options.Choice("Choice 1", "Choice 2", "Choice 3")]
		public readonly int tooltipChoice1 = 0;

		public enum ChoiceEnum { EnumChoice1, EnumChoice2, EnumChoice3};

		[Options.Field("Enum choice with tooltip", "Enum choice tooltip")]
		public readonly ChoiceEnum tooltipChoice2 = ChoiceEnum.EnumChoice1;


		class NewTooltip: Options.Components.Tooltip
		{
			public override string tooltip => _tooltip + UnityEngine.Random.value;
		}

		[Options.Field("Slider with tooltip", "Slider tooltip", typeof(NewTooltip))]
		[Field.Range(0, 100)]
		public readonly float tooltipSlider1 = 50f;
#endif

#if TEST_CUSTOM_FORMATS
		[Options.Field("Slider with custom value format")]
		[Options.Slider(defaultValue: 25f, valueFormat: "{0:F3}")]
		[Field.Range(0, 100)]
		public readonly float formatSlider1 = 50f;

		[Options.Field("Float slider")]
		[Options.Slider(valueFormat:"{0:F2}")]
		[Field.Range(0, 1)]
		public readonly float floatSlider = 0.5f;

		const string testFormat = "dsfdsf xxx {0:F0} %";

		[Options.Field("slider w/t", "TOOO")]
		[Options.Slider(valueFormat:testFormat)]
		[Field.Range(0, 100)]
		public readonly int percentSlider = 50;

		[Options.Field("slider wo/t")]
		[Options.Slider(valueFormat: testFormat)]
		[Field.Range(0, 100)]
		public readonly int percentSlider1 = 50;

		[Options.Field("Slider with big value")]
		[Options.Slider(valueFormat:"{0:F0}")]
		[Field.Range(0, 10000000000)]
		public readonly float bigSider0 = 0f;

		[Options.Field("Slider with ridiculously big description and value")]
		[Options.Slider(valueFormat:"{0:F0}")]
		[Field.Range(0, 10000000000)]
		public readonly float bigSider1 = 0f;
#endif

#if TEST_SLIDERS_RANGE
		[AddToConsole("range_test")]
		public class RangeTest
		{
			[Options.Field("1. Range [-10, 10]")]
			[Field.Range(-10, 10)]
			public readonly float slider1 = 50f;

			[Options.Field("2. Range [-10, 10], slider [-20, 20]")]
			[Field.Range(-10, 10)]
			[Options.Slider(defaultValue:0f, minValue: -20, maxValue: 20)]
			public readonly float slider2 = 0f;

			[Options.Field("3. Range [-10, 10], slider [-5, 5]")]
			[Field.Range(-10, 10)]
			[Options.Slider(defaultValue:0f, minValue: -5, maxValue: 5)]
			public readonly float slider3 = 0f;

			[Options.Field("4. Range [min:-10], slider [-5, 5]")]
			[Field.Range(min: -10)]
			[Options.Slider(defaultValue:0f, minValue: -5, maxValue: 5)]
			public readonly float slider4 = 0f;

			[Options.Field("5. Range [min:-10], slider [-20, 20]")]
			[Field.Range(min: -10)]
			[Options.Slider(defaultValue:0f, minValue: -20, maxValue: 20)]
			public readonly float slider5 = 0f;

			[Options.Field("6. Range [max:10], slider [-5, 5]")]
			[Field.Range(max: 10)]
			[Options.Slider(defaultValue:0f, minValue: -5, maxValue: 5)]
			public readonly float slider6 = 0f;

			[Options.Field("7. Range [max:10], slider [-20, 20]")]
			[Field.Range(max: 10)]
			[Options.Slider(defaultValue:0f, minValue: -20, maxValue: 20)]
			public readonly float slider7 = 0f;

			[Options.Field("8. Range [no], slider [-20, 20]")]
			[Options.Slider(defaultValue:0f, minValue: -20, maxValue: 20, valueFormat: "{0:F0}")]
			public readonly float slider8 = 50f;

			// not supposed to be added
			[Options.Field("9. Range [no], slider [min:-20]")]
			[Options.Slider(defaultValue:0f, minValue: -20)]
			public readonly float slider9 = 0f;
		}
		public readonly RangeTest rangeTest = new RangeTest();
#endif

#if TEST_CUSTOM_VALUE
		class CustomSliderValue: SMLHelper.V2.Options.ModSliderOption.SliderValue
		{
			protected override void UpdateLabel()
			{
				slider.value = UnityEngine.Mathf.Round(slider.value / 5.0f) * 5.0f;
				base.UpdateLabel();
			}
		}

		[Options.Field("Slider with custom value")]
		[Options.Slider(valueFormat:"{0:F0}", customValueType:typeof(CustomSliderValue))]
		[Field.Range(0, 100)]
		public readonly float customSlider = 0f;


		class CustomNonlinearSliderValue: Options.Components.SliderValue.Nonlinear
		{
			CustomNonlinearSliderValue()
			{
				addValueInfo(0.5f, 1.0f, "{0:F2}", "{0:F1}");
				addValueInfo(0.7f, 3.0f);
				addValueInfo(0.8f, 10.0f, "{0:F1}", "{0:F0}");
			}

			public override float ConvertToDisplayValue(float value) =>
				UnityEngine.Mathf.Round(base.ConvertToDisplayValue(value) * 100f) / 100f;
		}

		[Field.Range(0.01f, 100f)]
		[Options.Field("Non-linear slider")]
		[Options.Slider(valueFormat: "{0:F2}", defaultValue: 1.0f, customValueType: typeof(CustomNonlinearSliderValue))]
		public readonly float nonlinearSlider = 1.0f;
#endif

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

				$"field: '{field.Name}' fieldPath: '{path}'".logDbg();
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

#if TEST_CHOICES
		[Options.Field]
		[Options.Choice("Choice 1", "Choice 2", "Choice 3")]
		public readonly int simpleIntChoice = 1;

		[Options.Field]
		[Options.Choice("Choice 1", "Choice 2", "Choice 3")]
		public readonly float simpleFloatChoice = 2f;

		[Options.Field]
		[Options.Choice("Choice 1 (123)", 123, "Choice 2 (-321)", -321, "Choice 3 (50)", 50)]
		public readonly int customIntChoice = -321;

		[Options.Field]
		[Options.Choice("Choice 1 (0.25)", 0.25f, "Choice 2 (-5.75)", -5.75f, "Choice 3 (0.003)", 0.003f)]
		public readonly float customFloatChoice = -5.75f;

		public enum TestEnum1 { Zero, One, Two, Three };

		[Options.Field]
		public readonly TestEnum1 simpleEnumChoice = TestEnum1.One;

		public enum TestEnum2 { First_123 = 123, Second_n500 = -500, Third_100 = 100, Fourth_Auto_101 };

		[Options.Field]
		public readonly TestEnum2 customEnumChoice = TestEnum2.Second_n500;

		[Options.Field]
		[Options.Choice("Default", "Choice 2", "Choice 3")]
		public readonly float choiceIncorrectInitial = -100f;
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