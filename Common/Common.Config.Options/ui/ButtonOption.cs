using System;

using UnityEngine;
using UnityEngine.Events;

namespace Common.Configuration
{
	partial class Options
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class ButtonAttribute: Config.Field.LoadOnlyAttribute {}

		public class ButtonOption: ModOption
		{
			public ButtonOption(Config.Field cfgField, string label): base(cfgField, label) {}

			public override void addOption(Options options)
			{
				// HACK: SMLHelper don't have button options yet, so we add toggle and then change it to button in onGameObjectChange
				options.AddToggleOption(id, "", false);
			}

			public override void onValueChange(EventArgs e) {}

			public override void onGameObjectChange(GameObject go)
			{
				var panel = optionsPanelGameObject.GetComponentInChildren<uGUI_TabbedControlsPanel>();
				int modsTabIndex = panel.tabs.FindIndex(tab => tab.container == go.transform.parent);
				Debug.assert(modsTabIndex != -1);

				UnityEngine.Object.DestroyImmediate(go);

				panel.AddButton(modsTabIndex, label, new UnityAction(onClick));

				var transform = panel.tabs[modsTabIndex].container.transform;
				var newGO = transform.GetChild(transform.childCount - 1).gameObject;
				base.onGameObjectChange(newGO);
			}

			// cfgField will run attached actions when we change its value
			void onClick() => cfgField.value = (cfgField.value as int?) + 1;
		}
	}
}