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

				TooltipFactory.WriteDescription(sb, GravTrapObjectsType.getFrom(obj).techTypeListName);
			}
		}

		[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
		static class TooltipFactory_ItemActions_Patch
		{
			static readonly string buttons = (Main.config.useWheelClick? Strings.Mouse.middleButton: "") +
											((Main.config.useWheelClick && Main.config.useWheelScroll)? L10n.str("ids_or"): "") +
											 (Main.config.useWheelScroll? (Strings.Mouse.scrollUp + "/" + Strings.Mouse.scrollDown): "");

			static bool Prepare() => Main.config.useWheelClick || Main.config.useWheelScroll; // just in case

			static void Postfix(StringBuilder sb, InventoryItem item)
			{
				if (item.item.GetTechType().isGravTrap())
					TooltipFactory.WriteAction(sb, buttons, L10n.str("ids_switchObjectsType"));
			}
		}

		[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
		static class uGUIInventoryTab_OnPointerClick_Patch
		{
			static bool Prepare() => Main.config.useWheelClick;

			static void Postfix(InventoryItem item, int button)
			{
				if (button == 2 && item.item.GetTechType().isGravTrap())
					GravTrapObjectsType.getFrom(item.item.gameObject).techTypeListIndex++;
			}
		}

		[HarmonyPatch(typeof(GUIHand), "OnUpdate")]
		static class GUIHand_OnUpdate_Patch
		{
			static bool Prepare() => Main.config.extraGUIText;

			static void Postfix(GUIHand __instance)
			{
				if (!__instance.player.IsFreeToInteract() || !AvatarInputHandler.main.IsEnabled())
					return;

				if (__instance.GetTool() is PlayerTool tool && tool.pickupable.GetTechType().isGravTrap())
					HandReticle.main.SetUseTextRaw(tool.GetCustomUseText(), GravTrapObjectsType.getFrom(tool.gameObject).techTypeListName);
			}
		}

		[HarmonyPatch(typeof(Pickupable), "OnHandHover")]
		static class Pickupable_OnHandHover_Patch
		{
			static bool Prepare() => Main.config.extraGUIText;

			static void Postfix(Pickupable __instance)
			{
				if (__instance.GetTechType().isGravTrap())
					HandReticle.main.interactText2 = GravTrapObjectsType.getFrom(__instance.gameObject).techTypeListName;
			}
		}
	}
}