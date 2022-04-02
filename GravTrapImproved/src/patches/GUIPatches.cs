using System.Text;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

namespace GravTrapImproved
{
	static class GUIPatches
	{
		static class TypeListSwitcher
		{
			static readonly bool useKeys = Main.config.keyNext != KeyCode.None && Main.config.keyPrev != KeyCode.None;

			public static string getActionString()
			{
				if (Main.config.useWheelClick)
					return Strings.Mouse.middleButton;
				else if (useKeys)
					return $"{Main.config.keyNext}/{Main.config.keyPrev}";
				else
					return $"{Strings.Mouse.scrollUp}/{Strings.Mouse.scrollDown}";
			}

			public static int getChangeListDir()
			{
				if (Main.config.useWheelClick)
					return Input.GetKeyDown(KeyCode.Mouse2)? 1: 0;
				else if (useKeys)
					return Input.GetKeyDown(Main.config.keyNext)? 1: (Input.GetKeyDown(Main.config.keyPrev)? -1: 0);
				else
					return InputHelper.getMouseWheelDir();
			}
		}

		static bool isGravTrap(this TechType techType) => techType == TechType.Gravsphere || techType == GravTrapMK2.TechType;

		[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
		static class TooltipFactory_ItemCommons_Patch
		{
			static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
			{
				if (!techType.isGravTrap())
					return;

				var objectsType = GravTrapObjectsType.getFrom(obj);
				objectsType.techTypeListIndex += TypeListSwitcher.getChangeListDir();
				TooltipFactory.WriteDescription(sb, objectsType.techTypeListName);
			}
		}

		[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
		static class TooltipFactory_ItemActions_Patch
		{
			static readonly string buttons = TypeListSwitcher.getActionString();

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