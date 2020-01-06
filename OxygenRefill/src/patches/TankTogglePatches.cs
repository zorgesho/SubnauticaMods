using System.Text;

using UnityEngine;
using Harmony;

using Common;

namespace OxygenRefill
{
	using static OxygenTankUtils;

	[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
	static class TooltipFactory_ItemCommons_Patch
	{
		static void Postfix(StringBuilder sb, GameObject obj)
		{
			if (getTankInSlot()?.item.gameObject == obj && !isTankUsed(obj.GetComponent<Oxygen>()))
				TooltipFactory.WriteDescription(sb, L10n.str("ids_TankIsNotUsed"));
		}
	}

	[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
	static class TooltipFactory_ItemActions_Patch
	{
		static void Postfix(StringBuilder sb, InventoryItem item)
		{
			if (item == getTankInSlot())
				TooltipFactory.WriteAction(sb, Strings.Mouse.rightButton, L10n.str("ids_ToggleTankUsage"));
		}
	}

	[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
	static class uGUI_InventoryTab_OnPointerClick_Patch
	{
		static void Postfix(InventoryItem item, int button)
		{
			if (button == 1 && item == getTankInSlot())
				toggleTankUsage();
		}
	}

#if DEBUG
	[HarmonyPatch(typeof(OxygenManager), "RegisterSource")]
	static class OxygenManager_RegisterSource_Patch
	{
		static void Postfix(Oxygen src) => $"registering oxygen source {src.name}".onScreen().logDbg();
	}

	[HarmonyPatch(typeof(OxygenManager), "UnregisterSource")]
	static class OxygenManager_UnregisterSource_Patch
	{
		static void Postfix(Oxygen src) => $"unregistering oxygen source {src.name}".onScreen().logDbg();
	}
#endif
}