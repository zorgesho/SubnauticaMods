using System.Text;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

namespace OxygenRefill
{
	using static OxygenTankUtils;

	[PatchClass]
	static class TankTogglePatches
	{
		[HarmonyPostfix, HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
		static void TooltipFactory_ItemCommons_Postfix(StringBuilder sb, GameObject obj)
		{
			if (getTankInSlot()?.item.gameObject == obj && !isTankUsed(obj.GetComponent<Oxygen>()))
				TooltipFactory.WriteDescription(sb, L10n.str("ids_TankIsNotUsed"));
		}

		[HarmonyPostfix, HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
		static void TooltipFactory_ItemActions_Postfix(StringBuilder sb, InventoryItem item)
		{
			if (item == getTankInSlot())
				TooltipFactory.WriteAction(sb, Strings.Mouse.rightButton, L10n.str("ids_ToggleTankUsage"));
		}

		[HarmonyPostfix, HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
		static void uGUIInventoryTab_OnPointerClick_Postfix(InventoryItem item, int button)
		{
			if (button == 1 && item == getTankInSlot())
				toggleTankUsage();
		}
#if DEBUG
		[HarmonyPostfix, HarmonyPatch(typeof(OxygenManager), "RegisterSource")]
		static void registerOxygenSource_Postfix(Oxygen src) => $"registering oxygen source {src.name}".onScreen().logDbg();

		[HarmonyPostfix, HarmonyPatch(typeof(OxygenManager), "UnregisterSource")]
		static void unregisterOxygenSource_Postfix(Oxygen src) => $"unregistering oxygen source {src.name}".onScreen().logDbg();
#endif
	}
}