using System;
using Harmony;

using UnityEngine;

namespace MiscPatches
{
	// GUI patches
	//[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
	//public static class TooltipFactory_ItemCommons_Patch
	//{
	//	public static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
	//	{
	//		if (techType == TechType.Gravsphere)
	//			TooltipFactory.WriteDescription(sb, "Objects type: " + GravTrapObjectsType.getFrom(obj).getObjectsTypeAsString());
	//	}
	//}

	//[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
	//public static class TooltipFactory_ItemActions_Patch
	//{
	//	public static void Postfix(StringBuilder sb, InventoryItem item)
	//	{
	//		if (item.item.GetTechType() == TechType.Gravsphere)
	//			TooltipFactory.WriteAction(sb, "MMB", "switch objects type");
	//	}
	//}

		

	[HarmonyPatch(typeof(OxygenManager), "RegisterSource")]
	public static class uGUI_InventoryTab_saasdOnPointerClick_Patch
	{
		public static void Postfix(OxygenManager __instance, Oxygen src)
		{
			ErrorMessage.AddDebug("RegisterSource");
			//if (item.item.GetTechType() == TechType.Gravsphere && button == 2)
			//	GravTrapObjectsType.getFrom(item.item.gameObject).switchObjectsType();
		}
	}

	[HarmonyPatch(typeof(OxygenManager), "UnregisterSource")]
	public static class uGUI_InventoryTab_saasdfsdOnPointerClick_Patch
	{
		public static void Postfix(OxygenManager __instance, Oxygen src)
		{
			ErrorMessage.AddDebug("UnregisterSource");
			//if (item.item.GetTechType() == TechType.Gravsphere && button == 2)
			//	GravTrapObjectsType.getFrom(item.item.gameObject).switchObjectsType();
		}
	}


	[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
	public static class uGUI_InventoryTab_OnPointerClick_Patch
	{
		public static void Postfix(InventoryItem item, int button)
		{
			if (button == 1 && item == Inventory.main.equipment.GetItemInSlot("Tank"))
			{
				ErrorMessage.AddDebug("click tank " + button);
				Player.main.GetComponent<OxygenManager>().UnregisterSource(item.item.GetComponent<Oxygen>());
			}
			//if (item.item.GetTechType() == TechType.Gravsphere && button == 2)
			//	GravTrapObjectsType.getFrom(item.item.gameObject).switchObjectsType();
		}
	}



}