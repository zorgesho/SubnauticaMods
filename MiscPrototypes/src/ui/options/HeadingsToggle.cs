using System;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Common;

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

	static class States
	{
		public static Dictionary<string, bool> states = new Dictionary<string, bool>();

		public static bool getState(string name)
		{
			if (states.TryGetValue(name, out bool state))
				return state;

			return true;
		}
	}


	class HeadingToggler: MonoBehaviour
	{
		List<GameObject> childOptions = null;
		string headingName = null;

		bool state = true; // opened by default

		void init()
		{
			"INIT".log();
			headingName = transform.Find("Caption")?.GetComponent<Text>()?.text;
			headingName.log();

			childOptions = new List<GameObject>();
			
			for (int i = transform.GetSiblingIndex() + 1; i < transform.parent.childCount; i++)
			{
				GameObject option = transform.parent.GetChild(i).gameObject;

				if (option.GetComponent<HeadingToggler>())
					break;

				childOptions.Add(option);
			}
		}

		public void ensureState()
		{
			if (childOptions == null)
				init();

			if (state != States.getState(headingName))
			{
				toggle(States.getState(headingName));

				GetComponentInChildren<ToggleButtonClickHandler>().setForcedState(States.getState(headingName));

			}
		}

		public void toggle(bool val)
		{
			if (childOptions == null)
				init();

			childOptions.ForEach(option => option.SetActive(val));

			States.states[headingName] = state = val;
		}
	}
	
	class ToggleButtonClickHandler: MonoBehaviour, IPointerClickHandler
	{
		const float timeRotate = 0.1f;

		bool isOpen = true;
		bool isRotating = false;

		public void setForcedState(bool _isOpen)
		{
			isOpen = _isOpen;
			//transform.eulerAngles = new Vector3(isOpen?0: -90, 0, 0);
			transform.localEulerAngles = new Vector3(0, 0, isOpen?-90: 0);
				
				//new Vector3(0, 0, isOpen?0: -90);
		}

		void Clicked()
		{
			if (isRotating)
				return;

			StartCoroutine(smoothRotate(isOpen? 90: -90));
			SendMessageUpwards("toggle", (isOpen = !isOpen));
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


	// TODO: make one class, patching in init
	[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "Awake")]
	static class uGUITabbedControlsPanel_AddHeading_Patch11
	{
		static void Postfix(uGUI_TabbedControlsPanel __instance)
		{
			uGUITabbedControlsPanel_AddHeading_Patch.initPrefab(__instance);
		}
	}


	[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "AddHeading")]
	static class uGUITabbedControlsPanel_AddHeading_Patch
	{
		public static GameObject headingPrefab = null;

		public static void initPrefab(uGUI_TabbedControlsPanel panel)
		{
			if (headingPrefab) // TODO: check ingame prefab
				return;

			headingPrefab = Object.Instantiate(panel.headingPrefab);

			headingPrefab.name = "OptionHeadingToggleable";

			Text text = headingPrefab.GetComponentInChildren<Text>();
			text.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			//text.gameObject.AddComponent<OnHover>(); // for tooltips

			GameObject button = Object.Instantiate(panel.choiceOptionPrefab.getChild("Choice/Background/NextButton"));
			button.name = "HeadingToggleButton";
			button.AddComponent<ToggleButtonClickHandler>();
			RectTransform buttonTransform = button.transform as RectTransform;
			buttonTransform.anchorMin = (button.transform as RectTransform).anchorMin.setX(0f);
			buttonTransform.anchorMax = (button.transform as RectTransform).anchorMax.setX(0f);
			buttonTransform.pivot = new Vector2(Main.config.pivotX, Main.config.pivotY);
			buttonTransform.localEulerAngles = new Vector3(0, 0, -90);

			buttonTransform.SetParent(headingPrefab.transform, false);
			buttonTransform.SetAsFirstSibling();

			buttonTransform.localPosition = button.transform.localPosition.setX(Main.config.posX);
			buttonTransform.localPosition = button.transform.localPosition.setY(Main.config.posY);

			Transform rr = headingPrefab.transform.Find("Caption").transform;// as RectTransform;
			rr.localPosition = rr.localPosition.setX(Main.config.posX2);

			headingPrefab.AddComponent<HeadingClickHandler>(); // ?????
			headingPrefab.AddComponent<HeadingToggler>();
		}
		
		
		static bool Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex, string label)
		{
			if (tabIndex != uGUIOptionsPanel_AddTab_Patch.modsTabIndex)
				return true;

			GameObject newHeading = __instance.AddItem(tabIndex, headingPrefab, label);

			RectTransform rr = newHeading.transform.Find("Caption").transform as RectTransform;
			$"{rr.localPosition.x} {label}".onScreen();

			return false;
		}
	}

	[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "SetVisibleTab")]
	static class uGUITabbedControlsPanel_SetVisibleTab_Patch
	{
		static void Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex)
		{
			if (tabIndex != uGUIOptionsPanel_AddTab_Patch.modsTabIndex)
				return;

			__instance.tabs[tabIndex].container.GetComponent<VerticalLayoutGroup>().spacing = 5;

			Transform options = __instance.tabs[tabIndex].container.transform;

			for (int i = 0; i < options.childCount; i++)
			{
				if (options.GetChild(i).GetComponent<HeadingToggler>() is HeadingToggler headingToggler)
					headingToggler.ensureState();
			}
		}
	}
}