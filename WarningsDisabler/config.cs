using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using Common;
using Common.Configuration;

namespace WarningsDisabler
{
	[Options.Name("Warnings & messages")]
	class ModConfig: Config
	{
		class Messages
		{
			public bool enabled = true;

			[NoInnerFieldsAttrProcessing]
			public readonly HashSet<string> messages; // hashset to avoid duplicating string while loading

			public Messages(params string[] messages) => this.messages = new HashSet<string>(messages);

			public bool isMessageAllowed(string message) => enabled || !messages.Contains(message);
		}


		class MessageListTooltip: Options.Components.TooltipCached<bool>
		{
			protected Messages msgList;

			protected virtual void Awake()
			{
				msgList = parentOption.cfgField.parent as Messages;
				Debug.assert(msgList != null);
			}

			protected override bool needUpdate => isParamsChanged(msgList.enabled);

			class L10n: LanguageHelper
			{
				public static readonly string ids_thisMessage  = "This message is {0}:";
				public static readonly string ids_theseMessages = "These messages are {0}:";
				public static readonly string ids_enabled  = "<color=#00ff00ff>enabled</color>";
				public static readonly string ids_disabled = "<color=#ffff00ff>disabled</color>";
			}

			public override string tooltip
			{
				get
				{
					StringBuilder sb = new();

					string title = L10n.str(msgList.messages.Count == 1? "ids_thisMessage": "ids_theseMessages");
					title = title.format(L10n.str(msgList.enabled? "ids_enabled": "ids_disabled"));

					sb.Append(Mod.Consts.isGameSN? "<size=19>": null);
					sb.AppendLine($"<color=#ffffffff>{title}</color>");

					int i = 0;
					foreach (var msg in msgList.messages)
					{
						sb.Append("\"" + Language.main.Get(msg) + "\"");
						if (i++ != msgList.messages.Count - 1)
							sb.AppendLine();
					}
					sb.Append(Mod.Consts.isGameSN? "</size>": null);

					return sb.ToString();
				}
			}
		}


		class AddMessagesAttribute: Attribute, IFieldAttribute
		{
			readonly string label;

			public AddMessagesAttribute(string label) => this.label = label;

			public void process(object config, FieldInfo field)
			{
				Messages messages = field.GetValue(config) as Messages;
				Debug.assert(messages != null);

				if (messages.messages.Count == 0)
					return;

				Field cfgField = new (messages, nameof(Messages.enabled));

				Options.ToggleOption option = new (cfgField, label);
				option.addHandler(new Options.Components.Tooltip.Add(typeof(MessageListTooltip), null));

				Options.add(option);

				(Config.main as ModConfig).allMessages.Add(messages);
			}
		}

		[NonSerialized, NoInnerFieldsAttrProcessing]
		readonly List<Messages> allMessages = new();

		public bool isMessageAllowed(string message) => !allMessages.Exists(list => !list.isMessageAllowed(message));


		[Options.Field("Oxygen warnings", tooltipType: typeof(OxygenTooltip))]
		[Options.FinalizeAction(typeof(OxygenWarnings.HideOxygenHint))]
		public readonly bool oxygenWarningsEnabled = true;

		class OxygenTooltip: MessageListTooltip
		{
			protected override void Awake() =>
				msgList = new Messages("OxygenWarning10", "OxygenWarning30");

			protected override bool needUpdate =>
				isParamsChanged(msgList.enabled = Main.config.oxygenWarningsEnabled);
		}

#pragma warning disable IDE0052 // field is never read

#if GAME_BZ
		[AddMessages("Hypothermia warning")]
		readonly Messages hypothermiaWarning = new
		(
			"HypothermiaWarning" // "Hypothermia imminent."
		);
#endif
		[AddMessages("Food and water warnings")]
		readonly Messages foodWaterWarnings = new
		(
			"FoodLow",			// "Calorie intake recommended."
			"FoodVeryLow",		// "Seek calorie intake."
			"FoodCritical",		// "Emergency, starvation imminent. Seek calorie intake immediately."
			"WaterLow",			// "Seek fluid intake."
			"WaterVeryLow",		// "Seek fluid intake."
			"WaterCritical",	// "Seek fluid intake immediately."
			"VitalsOk"			// "Vital signs stabilizing."
		);

		[AddMessages("Depth warnings")]
		readonly Messages depthWarnings = new
		(
			"DepthWarning100",	// "Warning: Passing 100 meters. Oxygen efficiency decreased."
			"DepthWarning200",	// "Warning: Passing 200 meters. Oxygen efficiency greatly decreased.",
			"WarningCrushDepth" // "WARNING: Approaching crush depth of {0} meters. Hull damage imminent! Ascend ASAP!",
		);

		[AddMessages("Habitat power warnings")]
		readonly Messages powerWarnings = new
		(
			"BasePowerDown",		// "HABITAT: Warning, emergency power only."
			"BaseWelcomeNoPower",	// "HABITAT: Warning: Emergency power only. Oxygen production offline."
			"BasePowerUp"			// "HABITAT: Power restored. All primary systems online."
		);

		[AddMessages("Welcome messages")]
		readonly Messages welcomeMessages = new
		(
#if GAME_SN
			"CyclopsWelcomeAboard",				// "CYCLOPS: Welcome aboard captain. All systems online."
			"CyclopsWelcomeAboardAttention",	// "CYCLOPS: Welcome aboard captain. Some systems require attention."
			"SeamothWelcomeAboard",				// "Seamoth: Welcome aboard captain."
			"SeamothWelcomeNoPower",			// "Seamoth: Warning: Emergency power only. Oxygen production offline."
#elif GAME_BZ
			"SeatruckWelcomeAboard",			// "Welcome aboard captain."
#endif
			"ExosuitWelcomeAboard",				// "PRAWN: Welcome aboard captain."
			"ExosuitWelcomeNoPower",			// "PRAWN: Warning: Emergency power only. Oxygen production offline."
			"BaseWelcomeAboard"					// "HABITAT: Welcome aboard captain."
			//"BaseWelcomeNoPower"				//  Moved to powerWarnings list
		);

#if GAME_SN
		[AddMessages("Stillsuit equip message")]
		readonly Messages stillsuitMessage = new
		(
			"StillsuitEquipped"
		);
#elif GAME_BZ
		[Options.Field("Startup disclaimer", "Show disclaimer on startup")]
		public readonly bool showDisclaimer = true;
#endif
		[AddMessages("Custom messages")]
		readonly Messages customMessages = new() { enabled = false };

#pragma warning restore // IDE0052
	}
}