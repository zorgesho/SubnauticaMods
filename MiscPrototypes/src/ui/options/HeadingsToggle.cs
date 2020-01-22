using System;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Common;
using Common.Configuration;

namespace MiscPrototypes
{
	using Debug  = Common.Debug;
	using Object = UnityEngine.Object;

	[HarmonyPatch(typeof(uGUI_OptionsPanel), "AddTab")] // from ModsOptionsAdjusted
	static class uGUIOptionsPanel_AddTab_Patch
	{
		public static int modsTabIndex { get; private set; } = -1;
		public static bool isMainMenu { get; private set; } = true; // is options opened in main menu or in game

		static void Postfix(uGUI_OptionsPanel __instance, string label, int __result)
		{
			if (label == "Mods")
				modsTabIndex = __result;

			isMainMenu = (__instance.GetComponent<MainMenuOptions>() != null);
		}
	}


	static class ModOptionsHeadingsToggle
	{
		enum HeadingState { Collapsed, Expanded };

		static GameObject headingPrefab = null;

		static class StoredHeadingStates
		{
			class StatesConfig: Config
			{
				[Oculus.Newtonsoft.Json.JsonProperty]
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
			static readonly StatesConfig statesConfig = Config.tryLoad<StatesConfig>("heading_states.json", false, false);

			public static HeadingState get(string name) => statesConfig[name];
			public static void store(string name, HeadingState state) => statesConfig[name] = state;
		}


		static void initHeadingPrefab(uGUI_TabbedControlsPanel panel)
		{
			if (headingPrefab) // TODO: check ingame prefab
				return;

			headingPrefab = Object.Instantiate(panel.headingPrefab);
			headingPrefab.name = "OptionHeadingToggleable";
			headingPrefab.AddComponent<HeadingClickHandler>(); // ?????
			headingPrefab.AddComponent<HeadingToggle>();

			Transform captionTransform = headingPrefab.transform.Find("Caption");
			captionTransform.localPosition = new Vector3(45f, 0f, 0f);
			captionTransform.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

			GameObject button = Object.Instantiate(panel.choiceOptionPrefab.getChild("Choice/Background/NextButton"));
			button.name = "HeadingToggleButton";
			button.AddComponent<ToggleButtonClickHandler>();

			RectTransform buttonTransform = button.transform as RectTransform;
			buttonTransform.SetParent(headingPrefab.transform, false);
			buttonTransform.SetAsFirstSibling();
			buttonTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
			buttonTransform.localPosition = new Vector3(15f, -13f, 0f);
			buttonTransform.pivot = new Vector2(0.25f, 0.5f);
			buttonTransform.anchorMin = buttonTransform.anchorMax = new Vector2(0f, 0.5f);
		}

		#region components
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

			public void ensureState()
			{
				init();

				HeadingState storedState = StoredHeadingStates.get(headingName);

				if (headingState != storedState)
				{
					setState(storedState);
					GetComponentInChildren<ToggleButtonClickHandler>().setStateInstant(storedState);
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

			void Clicked()
			{
				if (isRotating)
					return;

				StartCoroutine(smoothRotate(headingState == HeadingState.Expanded? 90: -90));
				headingState = headingState == HeadingState.Expanded? HeadingState.Collapsed: HeadingState.Expanded;

				SendMessageUpwards("setState", headingState);
			}

			public void OnPointerClick(PointerEventData pointerEventData) => Clicked();

			IEnumerator smoothRotate(float angles)
			{
				isRotating = true;

				Quaternion startRotation = transform.localRotation;
				Quaternion endRotation = Quaternion.Euler(new Vector3(0f, 0f, angles)) * startRotation;

				for (float t = 0; t < timeRotate; t += Time.deltaTime)
				{
					transform.localRotation = Quaternion.Lerp(startRotation, endRotation, t / timeRotate);
					yield return null;
				}

				transform.localRotation = endRotation;
				isRotating = false;
			}
		}


		class HeadingClickHandler: MonoBehaviour, IPointerClickHandler
		{
			public void OnPointerClick(PointerEventData pointerEventData) => BroadcastMessage("Clicked");
		}
		#endregion

		#region patches for uGUI_TabbedControlsPanel
		// prefix for uGUI_TabbedControlsPanel.AddHeading  [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "AddHeading")]
		static bool _addHeading(uGUI_TabbedControlsPanel __instance, int tabIndex, string label)
		{
			if (tabIndex != uGUIOptionsPanel_AddTab_Patch.modsTabIndex)
				return true;

			__instance.AddItem(tabIndex, headingPrefab, label);
			return false;
		}

		// postfix for uGUI_TabbedControlsPanel.Awake  [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "Awake")]
		static void _awakeInitPrefab(uGUI_TabbedControlsPanel __instance)
		{
			initHeadingPrefab(__instance);
		}

		// prefix for uGUI_TabbedControlsPanel.SetVisibleTab  [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "SetVisibleTab")]
		static void _setVisibleTab(uGUI_TabbedControlsPanel __instance, int tabIndex)
		{
			if (tabIndex != uGUIOptionsPanel_AddTab_Patch.modsTabIndex)
				return;

			//__instance.tabs[tabIndex].container.GetComponent<VerticalLayoutGroup>().spacing = -5; // TODO

			Transform options = __instance.tabs[tabIndex].container.transform;

			for (int i = 0; i < options.childCount; i++)
				options.GetChild(i).GetComponent<HeadingToggle>()?.ensureState();
		}
		#endregion


		static bool inited = false;

		public static void init()
		{
			if (inited)
				return;

			inited = true;

			HarmonyHelper.patch(typeof(uGUI_TabbedControlsPanel).method("AddHeading"),
				prefix: typeof(ModOptionsHeadingsToggle).method(nameof(ModOptionsHeadingsToggle._addHeading)));

			HarmonyHelper.patch(typeof(uGUI_TabbedControlsPanel).method("Awake"),
				postfix: typeof(ModOptionsHeadingsToggle).method(nameof(ModOptionsHeadingsToggle._awakeInitPrefab)));

			HarmonyHelper.patch(typeof(uGUI_TabbedControlsPanel).method("SetVisibleTab"),
				prefix: typeof(ModOptionsHeadingsToggle).method(nameof(ModOptionsHeadingsToggle._setVisibleTab)));
		}
	}
}