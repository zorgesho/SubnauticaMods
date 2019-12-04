using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Common.UI
{
	static class UIExtensions
	{
		public static void addListener(this EventTrigger trigger, EventTriggerType eventID, UnityAction<BaseEventData> call)
		{
			var entry = new EventTrigger.Entry { eventID = eventID };
			entry.callback.AddListener(call);
			trigger.triggers.Add(entry);
		}

		//public static int getTextWidth(this Text text)
		//{																											//$"getTextWidth for text:'{text.text}'".logDbg();
		//	int width = 0;
 
		//	Font font = text.font; 
		//	font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);
 
		//	foreach (char c in text.text)
		//	{
		//		font.GetCharacterInfo(c, out CharacterInfo charInfo, text.fontSize, text.fontStyle);				//$"GetCharacterInfo {c} {charInfo.advance}".logDbg();
		//		width += charInfo.advance;
		//	}
		//																											$"textWidth: {width} '{text.text}'".logDbg();
		//	return width;
		//}
	}

	static class RectTransformExtensions
	{
		public static void setParams(this RectTransform rt, Vector2 anchor, Vector2 pivot, Transform parent = null)
		{
			rt.pivot = pivot;
			
			if (parent != null)
				rt.SetParent(parent, false);
			
			rt.anchorMin = anchor;
			rt.anchorMax = anchor;
			rt.localRotation = Quaternion.identity;
			rt.localPosition = Vector3.zero;
			rt.localScale = Vector3.one;
		}

		public static void setSize(this RectTransform rt, float width, float height)
		{
			rt.setWidth(width);
			rt.setHeight(height);
		}
		
		public static void setWidth(this RectTransform rt, float width) =>
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		
		public static void setHeight(this RectTransform rt, float height) =>
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}
}