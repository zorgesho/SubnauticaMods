using UnityEngine;
using UnityEngine.UI;
using Harmony;


using Common;

namespace MiscPrototypes
{
	static class TestPingToggles
	{
		public static void init()
		{
			GameObject pingTab = Player.main.GetPDA().ui.gameObject.getChild("Content/PingManagerTab/Content");
			GameObject buttonAll = pingTab.getChild("ButtonAll");

			GameObject button1 = Object.Instantiate(buttonAll);
			button1.setParent(pingTab, false);

			//button1.GetComponent<RectTransform>().copyValuesFrom(buttonAll.GetComponent<RectTransform>());
			button1.GetComponent<RectTransform>().anchoredPosition =  buttonAll.GetComponent<RectTransform>().anchoredPosition;
			button1.GetComponent<RectTransform>().anchoredPosition3D =  buttonAll.GetComponent<RectTransform>().anchoredPosition3D;
			button1.GetComponent<RectTransform>().localPosition =  buttonAll.GetComponent<RectTransform>().localPosition;
			button1.GetComponent<RectTransform>().localScale =  buttonAll.GetComponent<RectTransform>().localScale;
			button1.GetComponent<RectTransform>().offsetMin =  buttonAll.GetComponent<RectTransform>().offsetMin;
			button1.GetComponent<RectTransform>().offsetMax =  buttonAll.GetComponent<RectTransform>().offsetMax;

			button1.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
			button1.GetComponent<Toggle>().onValueChanged = null;
			//button1.GetComponent<Toggle>().onValueChanged.AddListener(delegate { ToggleValueChanged(null); });

			Vector3 pos = button1.transform.localPosition;
			pos.x += 50;
			button1.transform.localPosition = pos;
			//GameObject button1 = buttonAll;
			////button1.setParent(pingTab, false);
			//Vector3 pos = button1.transform.localPosition;
			//pos.x += 50;
			//button1.transform.localPosition = pos;
		}

		static void ToggleValueChanged(Toggle change)
		{
			"LLL".onScreen();
		}
	}
	
	[HarmonyPatch(typeof(uGUI_PingTab), "SetEntriesVisibility")]
	class Player_Update_Patchsdf
	{
		static void Postfix()
		{
			Common.Debug.logStack();
		}
	}

	[HarmonyPatch(typeof(Player), "Update")]
	class Player_Update_Patch
	{
		static void Postfix()
		{
			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				TestPingToggles.init();
			}
		}
	}
}