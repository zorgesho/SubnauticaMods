using System;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Common;

namespace ModsOptionsAdjusted
{
	class SmoothSetVisible: MonoBehaviour
	{
		public int state = 0;
		bool rotating = false;
		//const float d = 0.2f;

		public void setVisible(bool val, float delay)
		{
			if (val)
			{
				if (!rotating)
				{
					StartCoroutine(smoothVisible(true, Main.config.alpha, delay));
					state = 1;
				}
			}
			else
			{
				if (!rotating)
				{
					StartCoroutine(smoothVisible(false, Main.config.alpha, delay));
					state = 0;
				}
			}
		}

		IEnumerator smoothVisible(bool val, float duration, float delay)
		{
			rotating = true;

			float alphaBegin = val? 0f: 1.0f;
			float alphaEnd = val? 1.0f: 0f;

			CanvasRenderer[] canvases = gameObject.GetComponentsInChildren<CanvasRenderer>();

			yield return new WaitForSeconds(delay);

			$"canvases {canvases.Length}".log();
			
			for (float t = 0; t < duration; t+= Time.deltaTime)
			{
				foreach (var canvas in canvases)
					canvas.SetAlpha(Mathf.Lerp(alphaBegin, alphaEnd, t));
				yield return null;
			}

			foreach (var canvas in canvases)
				canvas.SetAlpha(alphaEnd);

			gameObject.SetActive(val);

			rotating = false;
		}

	}
	
	
	class SmoothRotate: MonoBehaviour
	{
		public int state = 0;
		bool rotating = false;
		const float d = 0.1f;

		public void rotate()
		{
			if (state == 0)
			{
				if (!rotating)
				{
					StartCoroutine(smoothRotate(new Vector3(-90, 0, 0), d));
					state = 1;
				}
			}
			else
			{
				if (!rotating)
				{
					StartCoroutine(smoothRotate(new Vector3(90, 0, 0), d));
					state = 0;
				}
			}
		}

		IEnumerator smoothRotate(Vector3 angles, float duration)
		{
			rotating = true;
			Quaternion startRotation = transform.rotation;
			Quaternion endRotation = Quaternion.Euler(angles) * startRotation;
			for (float t = 0; t < duration; t+= Time.deltaTime)
			{
				transform.rotation = Quaternion.Lerp(startRotation, endRotation, t / duration);
				yield return null;
			}

			transform.rotation = endRotation;
			rotating = false;
		}
	}
	
	class OnHover: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltip
	{
		public void OnPointerEnter(PointerEventData eventData)
		{
			$"ENTER {uGUI_Tooltip.main}".onScreen();
			
			"------------".log();
			
			foreach (var t in eventData.hovered)
			{
				t.name.log();
				if (t.name.Contains("NextButton"))
				{
					"^^^^^".log();
					uGUI_Tooltip.Set(null);
					return;
				}
			}

			//uGUI_Tooltip.Set(this);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			"EXIT".onScreen();
			uGUI_Tooltip.Set(null);
			
		}

		public void GetTooltip(out string tooltipText, List<TooltipIcon> tooltipIcons)
		{
//sb.AppendFormat("<size=25><color=#ffffffff>{0}</color></size>", title);
			tooltipText = $"<size=25><color=#ffffffff>OLOLO</color></size>" + " OLOLO\n PEPEPE\n VIVIVIIVIBO s dfls nasnd";
		}
		
	}


	[HarmonyPatch(typeof(uGUI_TabbedControlsPanel), "AddHeading")]
	static class uGUI_TabbedControlsPanel_AddHeading_Patch
	{
		static GameObject buttonPrefab = null;

		
		static void Postfix(uGUI_TabbedControlsPanel __instance, int tabIndex, string label)
		{
			if (tabIndex != uGUIOptionsPanel_AddTab_Patch.modsTabIndex)
				return;
				
			GameObject heading = __instance.tabs[tabIndex].container.transform.GetChild(__instance.tabs[tabIndex].container.transform.childCount - 1)?.gameObject;

			




			heading.GetComponentInChildren<Text>()?.text.onScreen();


			Text t = heading.GetComponentInChildren<Text>();
			//t.gameObject.AddComponent<LayoutElement>().minWidth = 300;;
			t.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // for autosizing captions
			t.gameObject.AddComponent<OnHover>();


			if (!buttonPrefab)
				buttonPrefab = __instance.choiceOptionPrefab.getChild("Choice/Background/NextButton");


			GameObject newBtt = UnityEngine.Object.Instantiate(buttonPrefab);
			(newBtt.transform as RectTransform).anchorMin = (newBtt.transform as RectTransform).anchorMin.setX(0f);
			(newBtt.transform as RectTransform).anchorMax = (newBtt.transform as RectTransform).anchorMax.setX(0f);
			(newBtt.transform as RectTransform).pivot = new Vector2(Main.config.pivotX, Main.config.pivotY);
			
			newBtt.transform.SetParent(heading.transform, false);
			newBtt.transform.SetAsFirstSibling();

			$"{newBtt.transform.localPosition}".onScreen();

			newBtt.transform.localPosition = newBtt.transform.localPosition.setX(Main.config.posX);
			newBtt.transform.localPosition = newBtt.transform.localPosition.setY(Main.config.posY);

			newBtt.AddComponent<SmoothRotate>();

			int iii = __instance.tabs[tabIndex].container.transform.childCount;

			//newBtt.GetComponent<Button>().
			
			newBtt.GetComponent<Button>().onClick.AddListener(() =>
			{
				newBtt.GetComponent<SmoothRotate>().rotate();

				if (newBtt.GetComponent<SmoothRotate>().state == 1)
				{
					for (int i = iii; i < iii + 3; i++)
					{
						__instance.tabs[tabIndex].container.GetChild(i).gameObject.SetActive(true);
						//__instance.tabs[tabIndex].container.GetChild(i).gameObject.ensureComponent<SmoothSetVisible>().setVisible(true, (i - iii)* Main.config.delay);
					}
				}
				else
				{
					for (int i = iii; i < iii + 3; i++)
					{
						__instance.tabs[tabIndex].container.GetChild(i).gameObject.SetActive(false);
						//__instance.tabs[tabIndex].container.GetChild(i).gameObject.ensureComponent<SmoothSetVisible>().setVisible(false, (i - iii)* Main.config.delay);
					}
				}
				//Vector3 rot = newBtt.transform.localEulerAngles;// = new Vector3(0, 0, -90);
				//if (rot.z == 270)
				//{
				//	newBtt.transform.localEulerAngles = new Vector3(0, 0, 0);

				//	__instance.tabs[tabIndex].container.GetChild(iii).gameObject.SetActive(false);
				//	__instance.tabs[tabIndex].container.GetChild(iii).gameObject.SetActive(false);
				//}
				//else
				//{
				//	newBtt.transform.localEulerAngles = new Vector3(0, 0, -90);
				//	__instance.tabs[tabIndex].container.GetChild(iii).gameObject.SetActive(true);

				//}
				
				$"{heading.GetComponentInChildren<Text>()?.text} {newBtt.transform.localEulerAngles}".onScreen();
			});

			RectTransform rr = heading.transform.Find("Caption").transform as RectTransform;

			rr.localPosition = rr.localPosition.setX(Main.config.posX2);
		}
	}

}
