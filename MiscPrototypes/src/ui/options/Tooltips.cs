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
}