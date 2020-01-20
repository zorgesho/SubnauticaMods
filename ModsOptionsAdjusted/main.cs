using System;
using System.Collections;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;

namespace ModsOptionsAdjusted
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.patchAll();
		}
	}


	[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "AddItem")]
	[HarmonyPatch(new Type[] {typeof(int), typeof(GameObject)})]
	static class uGUITabbedControlsPanel_AddItem_Patch
	{
		static readonly Tuple<string, Type>[] optionTypes = new Tuple<string, Type>[]
		{
			Tuple.Create("uGUI_ToggleOption",  typeof(AdjustToggleOption)),
			Tuple.Create("uGUI_SliderOption",  typeof(AdjustSliderOption)),
			Tuple.Create("uGUI_ChoiceOption",  typeof(AdjustChoiceOption)),
			Tuple.Create("uGUI_BindingOption", typeof(AdjustBindingOption))
		};

		//static void Postfix(uGUI_TabbedControlsPanel __instance, int tabIndex, GameObject optionPrefab, ref GameObject __result)
		static void Postfix(int tabIndex, ref GameObject __result)
		{
			if (__result == null)
				return;

			if (tabIndex != 4) // HACK ! TODO: !
				return;

			foreach (var type in optionTypes)
			{
				if (__result.name.Contains(type.Item1))
					__result.ensureComponent(type.Item2);
			}
		}

		// TODO: comment
		abstract class AdjustModOption: MonoBehaviour
		{
			const float minCaptionWidth = 200f; // !!!! TODO

			GameObject caption = null;
			protected float captionWidth { get => caption?.GetComponent<RectTransform>().rect.width ?? 0f; }

			protected void setCaptionGameObject(string gameObjectPath)
			{
				caption = gameObject.getChild(gameObjectPath);

				if (!caption && $"!! CAPTION {gameObjectPath}".logError())
					return;

				caption.AddComponent<LayoutElement>().minWidth = minCaptionWidth;
				caption.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // TODO ?????????

				RectTransform transform = caption.GetComponent<RectTransform>();
				transform.SetSiblingIndex(0); // TODO: comment
				transform.pivot = transform.pivot.setX(0f);
				transform.anchoredPosition = transform.anchoredPosition.setX(0f);
			}
		}

		class AdjustToggleOption: AdjustModOption
		{
			const float spacing = 20f;

			void Awake()
			{
				HorizontalLayoutGroup hlg = gameObject.getChild("Toggle").AddComponent<HorizontalLayoutGroup>();
				hlg.childControlWidth = false;
				hlg.childForceExpandWidth = false;
				hlg.childAlignment = TextAnchor.MiddleLeft;
				hlg.spacing = spacing;

				setCaptionGameObject("Toggle/Caption");
			}

			void Start() => Destroy(this);
		}

		class AdjustSliderOption: AdjustModOption
		{
			const float spacing = 25f;
			const float sliderValueWidth = 85f;

			IEnumerator Start()
			{
				setCaptionGameObject("Slider/Caption");
				yield return null; // skip one frame

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

				RectTransform rect = gameObject.transform.Find("Choice/Background").GetComponent<RectTransform>();

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

				RectTransform rect = gameObject.transform.Find("Bindings").GetComponent<RectTransform>();

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