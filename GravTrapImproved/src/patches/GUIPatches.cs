using System.Text;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

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

				if (InputHelper.getMouseWheelValue() != 0f) // not exactly right to do it here, but I didn't find a better way
					GravTrapObjectsType.getFrom(obj).techTypeListIndex += InputHelper.getMouseWheelDir();

				TooltipFactory.WriteDescription(sb, GravTrapObjectsType.getFrom(obj).techTypeListName);
			}
		}

		[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
		static class TooltipFactory_ItemActions_Patch
		{
			static readonly string buttons = Strings.Mouse.scrollUp + "/" + Strings.Mouse.scrollDown;

			static void Postfix(StringBuilder sb, InventoryItem item)
			{
				if (item.item.GetTechType().isGravTrap())
					TooltipFactory.WriteAction(sb, buttons, L10n.str("ids_switchObjectsType"));
			}
		}

		[PatchClass]
		static class ExtraGUITextPatch
		{
			static bool prepare() => Main.config.extraGUIText;

			[HarmonyPostfix, HarmonyPatch(typeof(GUIHand), "OnUpdate")]
			static void GUIHand_OnUpdate_Postfix(GUIHand __instance)
			{
				if (!__instance.player.IsFreeToInteract() || !AvatarInputHandler.main.IsEnabled())
					return;

				if (__instance.GetTool() is PlayerTool tool && tool.pickupable?.GetTechType().isGravTrap() == true)
					HandReticle.main.setText(textUse: tool.GetCustomUseText(), textUseSubscript: GravTrapObjectsType.getFrom(tool.gameObject).techTypeListName);
			}

			[HarmonyPostfix, HarmonyPatch(typeof(Pickupable), "OnHandHover")]
			static void Pickupable_OnHandHover_Postfix(Pickupable __instance)
			{
				if (__instance.GetTechType().isGravTrap())
					HandReticle.main.setText(textHandSubscript: GravTrapObjectsType.getFrom(__instance.gameObject).techTypeListName);
			}
		}
	}
}