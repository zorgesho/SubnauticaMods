using System;
using System.Text;

using Harmony;
using UnityEngine;

using Common;

namespace GravTrapImproved
{
	static class GUIPatches
	{
		static bool isGravTrap(this TechType techType) => techType == TechType.Gravsphere || techType == GravTrapMK2.TechType;

		[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
		static class TooltipFactory_ItemCommons_Patch
		{
			static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
			{
				if (!techType.isGravTrap())
					return;

				if (Main.config.useWheelScroll && InputHelper.getMouseWheelValue() != 0f) // not exactly right to do it here, but I didn't find a better way
					GravTrapObjectsType.getFrom(obj).techTypeListIndex += Math.Sign(InputHelper.getMouseWheelValue());

				TooltipFactory.WriteDescription(sb, "Objects type: " + GravTrapObjectsType.getFrom(obj).techTypeListName);
			}
		}

		[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
		static class TooltipFactory_ItemActions_Patch
		{
			static readonly string buttons = (Main.config.useWheelClick? Strings.Mouse.middleButton: "") +
											((Main.config.useWheelClick && Main.config.useWheelScroll)? " or ": "") +
											 (Main.config.useWheelScroll? (Strings.Mouse.scrollUp + "/" + Strings.Mouse.scrollDown): "");

			static void Postfix(StringBuilder sb, InventoryItem item)
			{
				if ((Main.config.useWheelClick || Main.config.useWheelScroll) && item.item.GetTechType().isGravTrap())
					TooltipFactory.WriteAction(sb, buttons, "switch objects type");
			}
		}

		[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
		static class uGUIInventoryTab_OnPointerClick_Patch
		{
			static bool Prepare() => Main.config.useWheelClick;

			static void Postfix(InventoryItem item, int button)
			{
				if (Main.config.useWheelClick && item.item.GetTechType().isGravTrap() && button == 2)
					GravTrapObjectsType.getFrom(item.item.gameObject).techTypeListIndex++;
			}
		}
	}
}