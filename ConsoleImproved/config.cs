using Common;
using Common.Harmony;
using Common.Configuration;

namespace ConsoleImproved
{
	class L10n: LanguageHelper
	{
		public static readonly string ids_matched = "Matched: ";
		public static readonly string ids_techType = "TechType: ";
		public static readonly string ids_findEntries = "Finded {0} entries";
		public static readonly string ids_listTooLarge = "List is too large ({0} entries), printing first {1} entries";

		public static readonly string ids_loremIpsum =
			"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
			"Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.<color=#00000000>{0}</color>";
	}

#if DEBUG
	[AddToConsole("console", true)]
#endif
	[Options.Name("Console & messages settings")]
	class ModConfig: Config
	{
		public readonly bool consoleEnabled = true;

		[Options.Field("Keep messages on the screen", "Keep messages on the screen while console is open")]
		[Options.FinalizeAction(typeof(UpdateOptionalPatches))]
		public readonly bool keepMessagesOnScreen = true;

		public readonly bool fixVanillaCommandsFloatParse = false;
		public readonly int  historySizeToSave = 100;

		[Field.Range(min: 0)]
		public readonly int maxListSize = 0; // 0 for max available

		[Field.Action(typeof(ErrorMessageSettings.RefreshSettings))]
		[Options.Hideable(typeof(Hider), "msgs")]
		public class MessagesSettings
		{
#pragma warning disable CS0414, CS0169, IDE0044 // field usage and readonly
			class Hider: Options.Components.Hider.Simple { public Hider(): base("msgs", () => Main.config.msgsSettings.customize) {} }

			class ClearMessages: Field.IAction
			{
				public void action()
				{
					if (Options.mode == Options.Mode.MainMenu)
						GameUtils.clearScreenMessages();
				}
			}

			[Options.Field("Customize messages settings")]
			[Field.Action(typeof(Hider))]
			[Field.Action(typeof(ClearMessages))]
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			public readonly bool customize = false;

			readonly bool detailedSettings = false;

			class SimpleSetting: Options.Components.Hider.IVisibilityChecker
			{ public bool visible => Main.config.msgsSettings.customize && !Main.config.msgsSettings.detailedSettings; }

			class DetailedSetting: Options.Components.Hider.IVisibilityChecker
			{ public bool visible => Main.config.msgsSettings.customize && Main.config.msgsSettings.detailedSettings; }

			class ButtonHider: Options.Components.Hider.IVisibilityChecker
			{ public bool visible => Main.config.msgsSettings.customize && Options.mode == Options.Mode.MainMenu; }

			class SampleMessage: Field.IAction
			{
				static int index = 0;
				public void action() => L10n.str("ids_loremIpsum").format(index++).onScreen();
			}

			[Options.Button]
			[Options.Field("Sample message")]
			[Field.Action(typeof(SampleMessage))]
			[Options.Hideable(typeof(ButtonHider), "msgs")]
			int _;

			[Options.Field("Font size")]
			[Field.Range(1, 60)]
			[Options.Slider(defaultValue: 18, minValue: 10, maxValue: 40)]
			public int fontSize = 18;

			[Options.Field("Offset", "Offset from the top left corner")]
			[Field.Range(-10f, 500f)]
			[Options.Slider(defaultValue: 140f, minValue: 5f, maxValue: 200f)]
			public float offset = 140f;

			[Options.Field("Message width", "Text width relative to the screen width")] 
			[Field.Range(0f, 1920f)]
			[Options.Slider(defaultValue: 500f, minValue: 200f, customValueType: typeof(Options.Components.SliderValue.RangePercent))]
			public float textWidth = 500f;

			class DelaySliderValue: Options.Components.SliderValue.Nonlinear
			{ DelaySliderValue() => addValueInfo(0.5f, 10.0f, "{0:F1} s", "{0:F0} s"); }

			[Options.Field("Message display time")]
			[Field.Range(min: 0.05f)]
			[Field.Action(typeof(ErrorMessageSettings.RefreshTimeDelaySetting))]
			[Options.Slider(defaultValue: 5f, maxValue: 60f, customValueType: typeof(DelaySliderValue))]
			public float timeDelay = 5f;

			[Options.Field("Line spacing", "Spacing between messages and between lines in multiline messages")]
			[Options.Hideable(typeof(SimpleSetting))]
			[Options.ChoiceMaster(0, nameof(messageSpacing), 10f, nameof(textLineSpacing), 1.2f)]
			[Options.ChoiceMaster(1, nameof(messageSpacing),  0f, nameof(textLineSpacing), 0.9f)]
			[Options.ChoiceMaster(2, nameof(messageSpacing), -5f, nameof(textLineSpacing), 0.75f)]
			[Options.Choice("Default", "Tight", "Compact")]
			readonly int _spacing = 0;

			[Options.Field("Animations speed", "Speed of fade out and fly animations")]
			[Options.Hideable(typeof(SimpleSetting))]
			[Options.ChoiceMaster(0, nameof(timeFly), 0.30f, nameof(timeFadeOut), 0.6f)]
			[Options.ChoiceMaster(1, nameof(timeFly), 0.15f, nameof(timeFadeOut), 0.3f)]
			[Options.ChoiceMaster(2, nameof(timeFly), 0.01f, nameof(timeFadeOut), 0.1f)]
			[Options.Choice("Default", "Fast", "Instant")]
			readonly int _animSpeed = 0;

			[Options.Field("Spacing between messages")]
			[Field.Range(-15f, 25f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 10f, minValue: -5f, maxValue: 15f, valueFormat: "{0:F1}")]
			public float messageSpacing = 10f;

			[Options.Field("Spacing between lines", "Spacing between lines in multiline messages (a value of 1 will produce normal line spacing)")]
			[Field.Range(0f, 2f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 1.2f, minValue: 0.5f, maxValue: 1.5f, valueFormat: "{0:F2}")]
			public float textLineSpacing = 1.2f;

			[Options.Field("Fly animation duration")]
			[Field.Range(0.01f, 2f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 0.3f, valueFormat: "{0:F2} s")]
			public float timeFly = 0.3f;

			[Options.Field("Fade out animation duration")]
			[Field.Range(0.1f, 5f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 0.6f, valueFormat: "{0:F2} s")]
			public float timeFadeOut = 0.6f;

			[Field.Range(min: 0.1f)]
			public float timeInvisible = 0.1f;
		}
		public MessagesSettings msgsSettings = new MessagesSettings();
	}
}