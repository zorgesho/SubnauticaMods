using System;
using System.Collections;

using SMLHelper.V2.Options;

namespace Common.Configuration
{
	using Reflection;

	partial class Options: ModOptions
	{
		public static class Utils
		{
			static readonly Type typeScrollRect = Type.GetType("UnityEngine.UI.ScrollRect, UnityEngine.UI");
			static readonly PropertyWrapper propScrollPos = typeScrollRect.property("verticalNormalizedPosition").wrap();
			static readonly MethodWrapper mtdSelectableSelect = Type.GetType("UnityEngine.UI.Selectable, UnityEngine.UI").method("Select").wrap();

			// recreates all ui controls in the options panel
			// keeps selected tab and scroll position
			public static void resetPanel()
			{
				if (!optionsPanel || !optionsPanel.enabled || optionsPanel.tabs.Count == 0)
					return;

				int currentTab = optionsPanel.currentTab;
				Debug.assert(currentTab < optionsPanel.tabs.Count);

				var scroll = optionsPanel.tabs[currentTab].pane.GetComponent(typeScrollRect);
				float scrollPos = propScrollPos.get<float>(scroll);

				optionsPanel.enabled = false; // all work is done by OnDisable() and OnEnable()
				optionsPanel.enabled = true;
				optionsPanel.SetVisibleTab(currentTab);

				scroll = optionsPanel.tabs[currentTab].pane.GetComponent(typeScrollRect); // new objects and components
				propScrollPos.set(scroll, scrollPos);
			}

			// open options menu and switch to the 'Mods' tab
			public static void open()
			{
				if (uGUI_MainMenu.main)
				{
					uGUI_MainMenu.main.OnButtonOptions();
				}
				else if (IngameMenu.main)
				{
					IngameMenu.main.Open();
					IngameMenu.main.ChangeSubscreen("Options");
				}

				optionsPanel.StartCoroutine(_selectModsTab());

				static IEnumerator _selectModsTab()
				{
					yield return null;
					mtdSelectableSelect.invoke(optionsPanel.tabs[modsTabIndex].getFieldValue("tabButton"));
				}
			}

			// scroll panel to show option at specified index
			// if index < 0 it uses for counting from the end of the list (e.g. '-1' is the last item)
			public static void scrollToShowOption(int index)
			{
				if (instance.modOptions.Count == 0)
					return;

				index = (index >= 0)? Math.Min(index, instance.modOptions.Count): Math.Max(0, instance.modOptions.Count + index);
				UIUtils.ScrollToShowItemInCenter(instance.modOptions[index].gameObject.transform);
			}
		}
	}
}