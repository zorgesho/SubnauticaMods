using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Harmony;

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


	[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "SetVisibleTab")]
	static class uGUITabbedControlsPanel_SetVisibleTab_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = new List<CodeInstruction>(cins);
			int index = list.FindIndex(ci => ci.isOp(OpCodes.Beq));

			if (index > 0)
				list.RemoveRange(index - 3, 4); // removing "this.currentTab != tabIndex" check

			return list.AsEnumerable();
		}

		// adjusting ui elements
		static void Postfix(uGUI_TabbedControlsPanel __instance, int tabIndex)
		{																								$"uGUI_TabbedControlsPanel.setVisibleTab tabIndex:{tabIndex}".logDbg();
			if (tabIndex < 0 || tabIndex >= __instance.tabs.Count)
				return;

			try
			{
				Transform options = __instance.tabs[tabIndex].container.transform;

				for (int i = 0; i < options.childCount; i++)
				{
					Transform option = options.GetChild(i);

					if (option.localPosition.x == 0) // layout don't adjust it yet
					{																					"uGUI_TabbedControlsPanel.setVisibleTab: continue".logDbg();
						continue;
					}

					if (option.name.Contains("uGUI_ToggleOption"))
						processToggleOption(option);
					else
					if (option.name.Contains("uGUI_SliderOption"))
						processSliderOption(option);
					else
					if (option.name.Contains("uGUI_ChoiceOption"))
						processChoiceOption(option);
					else
					if (option.name.Contains("uGUI_BindingOption"))
						processBindingOption(option);
				}
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}


		static void processToggleOption(Transform option)
		{
			Transform check = option.Find("Toggle/Background");
			Text text = option.GetComponentInChildren<Text>();																				$"processToggleOption {text.text}".logDbg();

			// :)
			if (text.text == "Enable AuxUpgradeConsole                        (Restart game)")
				text.text = "Enable AuxUpgradeConsole (Restart game)";

			int textWidth = text.getTextWidth() + 20;
			Vector3 pos = check.localPosition;

			if (textWidth > pos.x)
			{																																$"processToggleOption: ADJUSTING".logDbg();
				pos.x = textWidth;
				check.localPosition = pos;
			}
		}


		static void processSliderOption(Transform option)
		{
			const float sliderValueWidth = 85f;

			// changing width for slider value label
			RectTransform sliderValueRect = option.Find("Slider/Value").GetComponent<RectTransform>();
			Vector2 valueSize = sliderValueRect.sizeDelta;
			valueSize.x = sliderValueWidth;
			sliderValueRect.sizeDelta = valueSize;

			// changing width for slider
			Transform slider = option.Find("Slider/Background");
			Text text = option.GetComponentInChildren<Text>();																				$"processSliderOption {text.text}".logDbg();

			RectTransform rect = slider.GetComponent<RectTransform>();

			float widthAll = option.GetComponent<RectTransform>().rect.width;
			float widthSlider = rect.rect.width;
			float widthText = text.getTextWidth() + 25;

			if (widthText + widthSlider + sliderValueWidth > widthAll)
			{																																$"processSliderOption: ADJUSTING".logDbg();
				Vector2 size = rect.sizeDelta;
				size.x = widthAll - widthText - sliderValueWidth - widthSlider;
				rect.sizeDelta = size;
			}
		}


		static void processChoiceOption(Transform option)
		{
			Transform choice = option.Find("Choice/Background");
			Text text = option.GetComponentInChildren<Text>();																				$"processChoiceOption {text.text}".logDbg();

			RectTransform rect = choice.GetComponent<RectTransform>();

			float widthAll = option.GetComponent<RectTransform>().rect.width;
			float widthChoice = rect.rect.width;

			float widthText = text.getTextWidth() + 10;

			if (widthText + widthChoice > widthAll)
			{																																$"processChoiceOption: ADJUSTING".logDbg();
				Vector2 size = rect.sizeDelta;
				size.x = widthAll - widthText - widthChoice;
				rect.sizeDelta = size;
			}
		}
		
		
		static void processBindingOption(Transform option)
		{
			// changing width for keybinding option
			Transform binding = option.Find("Bindings");
			Text text = option.GetComponentInChildren<Text>();																				$"processBindingOption {text.text}".logDbg();

			RectTransform rect = binding.GetComponent<RectTransform>();

			float widthAll = option.GetComponent<RectTransform>().rect.width;
			float widthBinding = rect.rect.width;

			float widthText = text.getTextWidth() + 10;

			if (widthText + widthBinding > widthAll)
			{																																$"processBindingOption: ADJUSTING".logDbg();
				Vector2 size = rect.sizeDelta;
				size.x = widthAll - widthText - widthBinding;
				rect.sizeDelta = size;
			}

			// fixing bug where all keybinds show 'D' (after reselecting tab)
			Transform primaryBinding = binding.Find("Primary Binding"); // bug only on primary bindings
			Text bindingText = primaryBinding.Find("Label").GetComponent<Text>();

			if (bindingText.text == "D")
			{
				string buttonRawText = primaryBinding.GetComponent<uGUI_Binding>().value;
				
				if (uGUI.buttonCharacters.TryGetValue(buttonRawText, out string buttonText))
					bindingText.text = buttonText;
				else
					bindingText.text = buttonRawText;
			}
		}


		public static int getTextWidth(this Text text)
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