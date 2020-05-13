using System.Collections;

using UnityEngine;

using Common;
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
	[AddToConsole("console")]
#endif
	[Options.Name("Console & messages settings")]
	class ModConfig: Config
	{
		public readonly bool consoleEnabled = true;

		[Options.Field] // TODO
		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly bool keepMessagesOnScreen = true;

		public readonly bool fixVanillaCommandsFloatParse = false;
		public readonly int  historySizeToSave = 100;

		[Field.Range(min:0)]
		public readonly int maxListSize = 0; // 0 for max available

		[Field.Action(typeof(RefreshSettings))]
		[Options.Hideable(typeof(Hider), "msgs")]
		public class MessagesSettings
		{
#pragma warning disable CS0414, CS0169, IDE0044 // field usage and readonly
			class RefreshSettings: Field.IAction { public void action() => ErrorMessageSettings.refresh(!Main.config.msgsSettings.customize); }

			class Hider: Options.Components.Hider.Simple { public Hider(): base("msgs", () => Main.config.msgsSettings.customize) {} }

			class ClearMessages: Field.IAction
			{
				public void action()
				{
					if (Options.mode == Options.Mode.IngameMenu)
						return;

					GameUtils.clearScreenMessages();

					if (!Main.config.msgsSettings.customize)
						ErrorMessage.main.StartCoroutine(_addTestMessage());

					static IEnumerator _addTestMessage()
					{
						yield return new WaitForSeconds(0.05f); // time to apply settings before adding message
						SampleMessage.print();
					}
				}
			}

			[Options.Field] // TODO
			[Field.Action(typeof(Hider))]
			[Field.Action(typeof(ClearMessages))]
			[Options.Hideable(typeof(Options.Components.Hider.Ignore), "")]
			public readonly bool customize = true;

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
				public static void print() => L10n.str("ids_loremIpsum").format(index++).onScreen();

				public void action() => print();
			}

			[Options.Button]
			[Options.Field("Sample message")]
			[Field.Action(typeof(SampleMessage))]
			[Options.Hideable(typeof(ButtonHider), "msgs")]
			int _;

			[Options.Field] // TODO
			[Field.Range(1, 60)]
			[Options.Slider(defaultValue: 18, minValue: 10, maxValue: 40)]
			public int fontSize = 18;

			[Options.Field] // TODO
			[Field.Range(-10f, 500f)]
			[Options.Slider(defaultValue: 140f, minValue: 5f, maxValue: 200f)]
			public float offset = 140f;

			[Options.Field] // TODO
			[Field.Range(0f, 1920f)]
			[Options.Slider(defaultValue: 500f, minValue: 200f, customValueType: typeof(Options.Components.SliderValue.RangePercent))]
			public float textWidth = 500f;

			[Options.Field] // TODO
			[Field.Range(min: 0.05f)]
			[Options.Slider(defaultValue: 5f, maxValue: 60f, valueFormat: "{0:F1}")]
			public float timeDelay = 5f;

			[Options.Field("Line spacing")] // TODO tooltip
			[Options.Hideable(typeof(SimpleSetting))]
			[Options.ChoiceMaster(Spacing.Default, nameof(messageSpacing), 10f, nameof(textLineSpacing), 1.2f)]
			[Options.ChoiceMaster(Spacing.Tight,   nameof(messageSpacing),  0f, nameof(textLineSpacing), 0.9f)]
			[Options.ChoiceMaster(Spacing.Compact, nameof(messageSpacing), -5f, nameof(textLineSpacing), 0.75f)]
			readonly Spacing spacing = Spacing.Default;
			enum Spacing { Default, Tight, Compact };

			[Options.Field("Animations")] // TODO tooltip
			[Options.Hideable(typeof(SimpleSetting))]
			[Options.ChoiceMaster(AnimSpeed.Default, nameof(timeFly), 0.30f, nameof(timeFadeOut), 0.6f)]
			[Options.ChoiceMaster(AnimSpeed.Fast,	 nameof(timeFly), 0.15f, nameof(timeFadeOut), 0.3f)]
			[Options.ChoiceMaster(AnimSpeed.Instant, nameof(timeFly), 0.01f, nameof(timeFadeOut), 0.1f)]
			readonly AnimSpeed animSpeed = AnimSpeed.Default;
			enum AnimSpeed { Default, Fast, Instant };


			[Options.Field] // TODO
			[Field.Range(-15f, 25f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 10f, minValue: -5f, maxValue: 15f, valueFormat: "{0:F1}")]
			public float messageSpacing = 10f;

			[Options.Field] // TODO
			[Field.Range(0f, 2f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 1.2f, minValue: 0.5f, maxValue: 1.5f, valueFormat: "{0:F2}")]
			public float textLineSpacing = 1.2f;

			[Options.Field] // TODO
			[Field.Range(0.01f, 2f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 0.3f, valueFormat: "{0:F2}")]
			public float timeFly = 0.3f;

			[Options.Field] // TODO
			[Field.Range(0.1f, 5f)]
			[Options.Hideable(typeof(DetailedSetting))]
			[Options.Slider(defaultValue: 0.6f, valueFormat: "{0:F2}")]
			public float timeFadeOut = 0.15f;

			[Field.Range(min: 0.1f)]
			public float timeInvisible = 0.1f;
		}
		public MessagesSettings msgsSettings = new MessagesSettings();
	}
}