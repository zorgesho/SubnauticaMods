using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace ModsOptionsAdjusted
{
	// Class for collapsing/expanding options in 'Mods' tab
	// Options can be collapsed/expanded by clicking on mod's title or arrow button
	[PatchClass]
	static class ModOptionsHeadingsToggle
	{
		enum HeadingState { Collapsed, Expanded };

		static GameObject headingPrefab = null;

		static class StoredHeadingStates
		{
			class StatesConfig: Config
			{
				readonly Dictionary<string, HeadingState> states = new Dictionary<string, HeadingState>();

				public HeadingState this[string name]
				{
					get => states.TryGetValue(name, out HeadingState state)? state: HeadingState.Expanded;

					set
					{
						states[name] = value;
						save();
					}
				}
			}
			static readonly StatesConfig statesConfig = Config.tryLoad<StatesConfig>("heading_states.json", Config.LoadOptions.ForcedLoad);

			public static HeadingState get(string name) => statesConfig[name];
			public static void store(string name, HeadingState state) => statesConfig[name] = state;
		}

		// we add arrow button from Choice ui element to the options headings for collapsing/expanding
		static void initHeadingPrefab(uGUI_TabbedControlsPanel panel)
		{
			if (headingPrefab)
				return;

			headingPrefab = Object.Instantiate(panel.headingPrefab);
			headingPrefab.name = "OptionHeadingToggleable";
			headingPrefab.AddComponent<HeadingToggle>();

			Transform captionTransform = headingPrefab.transform.Find("Caption");
			captionTransform.localPosition = new Vector3(45f, 0f, 0f);
			captionTransform.gameObject.AddComponent<HeadingClickHandler>();
			captionTransform.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

			GameObject button = Object.Instantiate(panel.choiceOptionPrefab.getChild("Choice/Background/NextButton"));
			button.name = "HeadingToggleButton";
			button.AddComponent<ToggleButtonClickHandler>();

			RectTransform buttonTransform = button.transform as RectTransform;
			buttonTransform.SetParent(headingPrefab.transform);
			buttonTransform.SetAsFirstSibling();
			buttonTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
			buttonTransform.localPosition = new Vector3(15f, -13f, 0f);
			buttonTransform.pivot = new Vector2(0.25f, 0.5f);
			buttonTransform.anchorMin = buttonTransform.anchorMax = new Vector2(0f, 0.5f);
		}

		#region components
		// main component for headings toggling
		class HeadingToggle: MonoBehaviour
		{
			HeadingState headingState = HeadingState.Expanded;
			string headingName = null;

			List<GameObject> childOptions = null;

			void init()
			{
				if (childOptions != null)
					return;

				headingName = transform.Find("Caption")?.GetComponent<Text>()?.text;

				childOptions = new List<GameObject>();

				for (int i = transform.GetSiblingIndex() + 1; i < transform.parent.childCount; i++)
				{
					GameObject option = transform.parent.GetChild(i).gameObject;

					if (option.GetComponent<HeadingToggle>())
						break;

					childOptions.Add(option);
				}
			}

			public void ensureState() // for setting previously saved state
			{
				init();

				HeadingState storedState = StoredHeadingStates.get(headingName);

				if (headingState != storedState)
				{
					setState(storedState);
					GetComponentInChildren<ToggleButtonClickHandler>()?.setStateInstant(storedState);
				}
			}

			public void setState(HeadingState state)
			{
				init();

				childOptions.ForEach(option => option.SetActive(state == HeadingState.Expanded));
				headingState = state;

				StoredHeadingStates.store(headingName, state);
			}
		}

		// click handler for arrow button
		class ToggleButtonClickHandler: MonoBehaviour, IPointerClickHandler
		{
			const float timeRotate = 0.1f;

			HeadingState headingState = HeadingState.Expanded;
			bool isRotating = false;

			public void setStateInstant(HeadingState state)
			{
				headingState = state;
				transform.localEulerAngles = new Vector3(0, 0, headingState == HeadingState.Expanded? -90: 0);
			}

			public void OnPointerClick(PointerEventData _)
			{
				if (isRotating)
					return;

				headingState = headingState == HeadingState.Expanded? HeadingState.Collapsed: HeadingState.Expanded;
				StartCoroutine(smoothRotate(headingState == HeadingState.Expanded? -90: 90));

				GetComponentInParent<HeadingToggle>()?.setState(headingState);
			}

			IEnumerator smoothRotate(float angles)
			{
				isRotating = true;

				Quaternion startRotation = transform.localRotation;
				Quaternion endRotation = Quaternion.Euler(new Vector3(0f, 0f, angles)) * startRotation;

				float timeStart = Time.realtimeSinceStartup; // Time.deltaTime works only in main menu

				while (timeStart + timeRotate > Time.realtimeSinceStartup)
				{
					transform.localRotation = Quaternion.Lerp(startRotation, endRotation, (Time.realtimeSinceStartup - timeStart) / timeRotate);
					yield return null;
				}

				transform.localRotation = endRotation;
				isRotating = false;
			}
		}

		// click handler for title, just redirects clicks to button click handler
		class HeadingClickHandler: MonoBehaviour, IPointerClickHandler
		{
			public void OnPointerClick(PointerEventData eventData) =>
				transform.parent.GetComponentInChildren<ToggleButtonClickHandler>()?.OnPointerClick(eventData);
		}
		#endregion

		#region patches for uGUI_TabbedControlsPanel
		[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "AddHeading")][HarmonyPrefix]
		static bool _addHeading(uGUI_TabbedControlsPanel __instance, int tabIndex, string label)
		{
			if (tabIndex != OptionsPanelInfo.modsTabIndex)
				return true;

			__instance.AddItem(tabIndex, headingPrefab, label);
			return false;
		}

		[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "Awake")][HarmonyPostfix]
		static void _awakeInitPrefab(uGUI_TabbedControlsPanel __instance)
		{
			initHeadingPrefab(__instance);
		}

		[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "SetVisibleTab")][HarmonyPrefix]
		static void _setVisibleTab(uGUI_TabbedControlsPanel __instance, int tabIndex)
		{
			if (tabIndex != OptionsPanelInfo.modsTabIndex)
				return;

			__instance.tabs[tabIndex].container.GetComponent<VerticalLayoutGroup>().spacing = Main.config.spacingModOptions;

			Transform options = __instance.tabs[tabIndex].container.transform;

			for (int i = 0; i < options.childCount; i++)
				options.GetChild(i).GetComponent<HeadingToggle>()?.ensureState();
		}
		#endregion
	}
}