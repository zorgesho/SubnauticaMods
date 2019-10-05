using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using Harmony;

namespace ModsOptionsAdjusted
{
	static public class Main
	{
		static public void patch()
		{
			HarmonyInstance.Create("ModsOptionsAdjusted").PatchAll(Assembly.GetExecutingAssembly());
		}
	}


	[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "SetVisibleTab")]
	static class uGUITabbedControlsPanel_SetVisibleTab_Patch
	{
		static bool Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex)
		{
			return !(tabIndex >= 0 && tabIndex < __instance.tabs.Count && __instance.tabs[tabIndex].pane.activeSelf);
		}
		
		static void Postfix(uGUI_TabbedControlsPanel __instance, int tabIndex)
		{
			if (tabIndex >= 0 && tabIndex < __instance.tabs.Count)
			{
				try
				{
					Transform options = __instance.tabs[tabIndex].container.transform;

					for (int i = 0; i < options.childCount; ++i)
					{
						Transform option = options.GetChild(i);

						if (option.localPosition.x == 0) // layout don't adjust it yet
							continue;

						if (option.name.Contains("uGUI_ToggleOption"))
							processToggleOption(option);
						else
						if (option.name.Contains("uGUI_SliderOption"))
							processSliderOption(option);
						else
						if (option.name.Contains("uGUI_ChoiceOption"))
							processChoiceOption(option);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"[ModsOptionsAdjusted] EXCEPTION: {e.GetType()}\t{e.Message}");
				}
			}
		}


		static void processToggleOption(Transform option)
		{
			Transform check = option.Find("Toggle/Background");
			Text text = option.GetComponentInChildren<Text>();

			// :)
			if (text.text == "Enable AuxUpgradeConsole                        (Restart game)")
				text.text = "Enable AuxUpgradeConsole (Restart game)";

			int textWidth = text.getTextWidth() + 20;
			Vector3 pos = check.localPosition;

			if (textWidth > pos.x)
			{
				pos.x = textWidth;
				check.localPosition = pos;
			}
		}

		static void processSliderOption(Transform option)
		{
			Transform slider = option.Find("Slider/Background");
			Text text = option.GetComponentInChildren<Text>();

			RectTransform rect = slider.GetComponent<RectTransform>();

			float widthAll = option.GetComponent<RectTransform>().rect.width;
			float widthValue = option.Find("Slider/Value").GetComponent<RectTransform>().rect.width;
			float widthSlider = rect.rect.width;

			float widthText = text.getTextWidth() + 25;

			if (widthText + widthSlider + widthValue > widthAll)
			{
				Vector2 size = rect.sizeDelta;
				size.x = widthAll - widthText - widthValue - widthSlider;
				rect.sizeDelta = size;
			}
		}

		static void processChoiceOption(Transform option)
		{
			Transform choice = option.Find("Choice/Background");
			Text text = option.GetComponentInChildren<Text>();

			RectTransform rect = choice.GetComponent<RectTransform>();

			float widthAll = option.GetComponent<RectTransform>().rect.width;
			float widthChoice = rect.rect.width;

			float widthText = text.getTextWidth() + 10;

			if (widthText + widthChoice > widthAll)
			{
				Vector2 size = rect.sizeDelta;
				size.x = widthAll - widthText - widthChoice;
				rect.sizeDelta = size;
			}
		}

		static public int getTextWidth(this Text text)
		{
			int width = 0;
 
			Font font = text.font; 
			font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);
 
			foreach (char c in text.text)
			{
				font.GetCharacterInfo(c, out CharacterInfo charInfo, text.fontSize, text.fontStyle);
				width += charInfo.advance;
			}

			return width;
		}
	}
}