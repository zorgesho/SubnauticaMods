using System;
using System.Reflection;
using System.Collections.Generic;

using Common;
using Common.Configuration;

namespace WarningsDisabler
{
	[Options.Name("Warnings & messages <color=#CCCCCCFF>(uncheck boxes to disable)</color>")]
	class ModConfig: Config
	{
		class Messages
		{
			public bool enabled = true;
			readonly HashSet<string> messages = null;

			public Messages(params string[] _messages) => messages = new HashSet<string>(_messages);

			public bool isEmpty() => messages.Count == 0;
			public bool isMessageAllowed(string message) => enabled || !messages.Contains(message);
		}

		class AddMessagesAttribute: SkipRecursiveAttrProcessing, IFieldAttribute
		{
			readonly string label;

			public AddMessagesAttribute(string _label) => label = _label;

			public void process(object config, FieldInfo field)
			{
				Messages messages = field.GetValue(config) as Messages;
				Debug.assert(messages != null);

				if (!messages.isEmpty())
				{
					var cfgField = new Field(messages, nameof(Messages.enabled));
					Options.add(new Options.ToggleOption(cfgField, label));

					(config as ModConfig)?.allMessages.Add(messages);
				}
			}
		}

		[NonSerialized]
		[SkipRecursiveAttrProcessing]
		readonly List<Messages> allMessages = new List<Messages>();

		public bool isMessageAllowed(string message) => !allMessages.Exists(list => !list.isMessageAllowed(message));


		[Options.Field("Oxygen warnings")]
		[Field.CustomAction(typeof(OxygenWarnings.HideOxygenHint))]
		public readonly bool oxygenWarningsEnabled = true;

#pragma warning disable IDE0052 // field is never read

		[AddMessages("Food and water warnings")]
		readonly Messages foodWaterWarnings = new Messages
		(
			"VitalsOk",			// "Vital signs stabilizing."
			"FoodLow",			// "Calorie intake recommended."
			"FoodCritical",		// "Emergency, starvation imminent. Seek calorie intake immediately."
			"FoodVeryLow",		// "Seek calorie intake."
			"WaterLow",			// "Seek fluid intake."
			"WaterCritical",	// "Seek fluid intake immediately."
			"WaterVeryLow"		// "Seek fluid intake."
		);

		[AddMessages("Depth warnings")]
		readonly Messages depthWarnings = new Messages
		(
			"DepthWarning100",	// "Warning: Passing 100 meters. Oxygen efficiency decreased."
			"DepthWarning200"	// "Warning: Passing 200 meters. Oxygen efficiency greatly decreased."
		);

		[AddMessages("Habitat power warnings")]
		readonly Messages powerWarnings = new Messages
		(
			"BasePowerUp",			// "HABITAT: Power restored. All primary systems online."
			"BasePowerDown",		// "HABITAT: Warning, emergency power only."
			"BaseWelcomeNoPower"	// "HABITAT: Warning: Emergency power only. Oxygen production offline."
		);

		[AddMessages("Welcome messages")]
		readonly Messages welcomeMessages = new Messages
		(
			"CyclopsWelcomeAboard",				// "CYCLOPS: Welcome aboard captain. All systems online."
			"CyclopsWelcomeAboardAttention",	// "CYCLOPS: Welcome aboard captain. Some systems require attention."
			"SeamothWelcomeAboard",				// "Seamoth: Welcome aboard captain."
			"SeamothWelcomeNoPower",			// "Seamoth: Warning: Emergency power only. Oxygen production offline."
			"ExosuitWelcomeAboard",				// "PRAWN: Welcome aboard captain."
			"ExosuitWelcomeNoPower",			// "PRAWN: Warning: Emergency power only. Oxygen production offline."
			"BaseWelcomeAboard"					// "HABITAT: Welcome aboard captain."
			//"BaseWelcomeNoPower"				//  Moved to powerWarnings list
		);

		[AddMessages("Stillsuit equip message")]
		readonly Messages stillsuitMessage = new Messages
		(
			"StillsuitEquipped"
		);

		[AddMessages("Custom messages")]
		readonly Messages customMessages = new Messages { enabled = false };

#pragma warning restore
	}
}