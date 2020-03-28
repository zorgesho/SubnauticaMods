using System;
using System.Collections;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;

namespace ModsOptionsAdjusted
{
	// Adjusting mod options ui elements so they don't overlap with their text labels.
	// We add corresponding 'adjuster' components to each ui element in mod options tab.
	// Reason for using components is to skip one frame before manually adjust ui elements to make sure that Unity UI Layout components is updated
	[HarmonyHelper.PatchClass]
	static class ModOptionsAdjuster
	{
		static readonly Tuple<string, Type>[] optionTypes = new Tuple<string, Type>[]
		{
			Tuple.Create("uGUI_ToggleOption",  typeof(AdjustToggleOption)),
			Tuple.Create("uGUI_SliderOption",  typeof(AdjustSliderOption)),
			Tuple.Create("uGUI_ChoiceOption",  typeof(AdjustChoiceOption)),
			Tuple.Create("uGUI_BindingOption", typeof(AdjustBindingOption))
		};

		[HarmonyPostfix]
		[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "AddItem", new Type[] {typeof(int), typeof(GameObject)})]
		static void _addItem(int tabIndex, GameObject __result)
		{
			if (__result == null || tabIndex != OptionsPanelInfo.modsTabIndex)
				return;

			foreach (var type in optionTypes)
			{
				if (__result.name.Contains(type.Item1))
				{
					__result.ensureComponent(type.Item2);
					break;
				}
			}
		}

		// base class for 'adjuster' components
		// we add ContentSizeFitter component to text label so it will change width in its Update() based on text
		// that's another reason to skip one frame
		abstract class AdjustModOption: MonoBehaviour
		{
			const float minCaptionWidth_MainMenu = 480f;
			const float minCaptionWidth_InGame   = 360f;

			GameObject caption = null;
			protected float captionWidth => caption?.GetComponent<RectTransform>().rect.width ?? 0f;

			protected void setCaptionGameObject(string gameObjectPath)
			{
				caption = gameObject.getChild(gameObjectPath);

				if (!caption && $"AdjustModOption: caption gameobject '{gameObjectPath}' not found".logError())
					return;

				caption.AddComponent<LayoutElement>().minWidth = OptionsPanelInfo.isMainMenu? minCaptionWidth_MainMenu: minCaptionWidth_InGame;
				caption.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // for autosizing captions

				RectTransform transform = caption.GetComponent<RectTransform>();
				transform.SetAsFirstSibling(); // for HorizontalLayoutGroup
				transform.pivot = transform.pivot.setX(0f);
				transform.anchoredPosition = transform.anchoredPosition.setX(0f);
			}
		}

		// in case of ToggleOption there is no need to manually move elements
		// other option types don't work well with HorizontalLayoutGroup :(
		class AdjustToggleOption: AdjustModOption
		{
			const float spacing = 20f;

			void Awake()
			{
				HorizontalLayoutGroup hlg = gameObject.getChild("Toggle").AddComponent<HorizontalLayoutGroup>();
				hlg.childControlWidth = false;
				hlg.childForceExpandWidth = false;
				hlg.spacing = spacing;

				setCaptionGameObject("Toggle/Caption");

				Destroy(this);
			}
		}

		class AdjustSliderOption: AdjustModOption
		{
			const float spacing = 25f;
			const float sliderValueWidth = 85f;

			IEnumerator Start()
			{
				setCaptionGameObject("Slider/Caption");
				yield return null; // skip one frame

				// for some reason sliders don't update their handle positions sometimes
				uGUI_SnappingSlider slider = gameObject.GetComponentInChildren<uGUI_SnappingSlider>();
				typeof(Slider).method("UpdateVisuals")?.Invoke(slider, null);

				// changing width for slider value label
				RectTransform sliderValueRect = gameObject.transform.Find("Slider/Value") as RectTransform;
				sliderValueRect.sizeDelta = sliderValueRect.sizeDelta.setX(sliderValueWidth);

				// changing width for slider
				RectTransform rect = gameObject.transform.Find("Slider/Background") as RectTransform;

				float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
				float widthSlider = rect.rect.width;
				float widthText = captionWidth + spacing;

				if (widthText + widthSlider + sliderValueWidth > widthAll)
					rect.sizeDelta = rect.sizeDelta.setX(widthAll - widthText - sliderValueWidth - widthSlider);

				Destroy(this);
			}
		}

		class AdjustChoiceOption: AdjustModOption
		{
			const float spacing = 10f;

			IEnumerator Start()
			{
				setCaptionGameObject("Choice/Caption");
				yield return null; // skip one frame

				RectTransform rect = gameObject.transform.Find("Choice/Background") as RectTransform;

				float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
				float widthChoice = rect.rect.width;
				float widthText = captionWidth + spacing;

				if (widthText + widthChoice > widthAll)
					rect.sizeDelta = rect.sizeDelta.setX(widthAll - widthText - widthChoice);

				Destroy(this);
			}
		}

		class AdjustBindingOption: AdjustModOption
		{
			const float spacing = 10f;

			IEnumerator Start()
			{
				setCaptionGameObject("Caption");
				yield return null; // skip one frame

				RectTransform rect = gameObject.transform.Find("Bindings") as RectTransform;

				float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
				float widthBinding = rect.rect.width;
				float widthText = captionWidth + spacing;

				if (widthText + widthBinding > widthAll)
					rect.sizeDelta = rect.sizeDelta.setX(widthAll - widthText - widthBinding);

				Destroy(this);
			}
		}
	}
}