using System;

using UnityEngine;
using UnityEngine.Events;

namespace Common.Configuration
{
	using Reflection;

	partial class Options
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class ButtonAttribute: Config.Field.LoadOnlyAttribute {}

		partial class Factory
		{
			class ButtonOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type != typeof(int) || cfgField.getAttr<ButtonAttribute>() == null) // it's good enough for now
						return null;

					return new ButtonOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label);
				}
			}
		}


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

			// using reflection to avoid including UnityEngine.UI in all projects
			static readonly Type eventSystem = ReflectionHelper.safeGetType("UnityEngine.UI", "UnityEngine.EventSystems.EventSystem");
			static readonly PropertyWrapper currentEventSystem = eventSystem.property("current").wrap();
			static readonly MethodWrapper setSelectedGameObject = eventSystem.method("SetSelectedGameObject", typeof(GameObject)).wrap();

			void onClick()
			{
				cfgField.value = (cfgField.value as int?) + 1; // cfgField will run attached actions when we change its value

				setSelectedGameObject.invoke(currentEventSystem.get(), null); // so button don't stays pressed after click
			}
		}
	}
}