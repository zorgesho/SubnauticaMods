using System.Text;

using UnityEngine;
using UnityEngine.UI;
using Harmony;

using Common;

namespace RenameBeacons
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.patchAll(false);
		}
	}


	[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
	static class TooltipFactory_ItemActions_Beacon_Patch
	{
		static void Postfix(StringBuilder sb, InventoryItem item)
		{
			if (item.item.GetTechType() == TechType.Beacon)
				TooltipFactory.WriteAction(sb, Strings.Mouse.middleButton, "rename");
		}
	}

	[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
	static class TooltipFactory_ItemCommons_Beacon_Patch
	{
		static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
		{
			if (techType == TechType.Beacon)
				TooltipFactory.WriteDescription(sb, $"Name: \"{obj.GetComponent<Beacon>().label}\"");
		}
	}
	
	[HarmonyPatch(typeof(Beacon), "OnPickedUp")]
	static class Beacon_OnPickedUp_Patch
	{
		static void Postfix(Beacon __instance)
		{
			__instance.label = __instance.beaconLabel.GetLabel();
		}
	}

	[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
	static class uGUI_InventoryTab_OnPointerClick_Beacon_Patch
	{
		static class BeaconRenamer
		{
			static Beacon beacon = null;

			public static void init(Beacon b)
			{
				beacon = b;
				if (beacon)
					beacon.beaconLabel.SetLabel(beacon.label);
			}

			public static void setLabel(string label)
			{
				if (beacon)
				{
					beacon.label = label;
					beacon.beaconLabel.SetLabel(label);
					beacon = null;
				}
			}
		}
		
		static void Postfix(InventoryItem item, int button)
		{
			if (item.item.GetTechType() == TechType.Beacon && button == 2)
			{
				Beacon beacon = item.item.GetComponent<Beacon>();
				BeaconRenamer.init(beacon);
				uGUI.main.userInput.RequestString(	Language.main.Get("BeaconLabel"),
													Language.main.Get("BeaconSubmit"),
													beacon.beaconLabel.labelName, 25,
													new uGUI_UserInput.UserInputCallback(BeaconRenamer.setLabel));
				
				// just polish
				uGUI.main.userInput.gameObject.GetComponentInChildren<Button>()?.OnDeselect(null);
				uGUI.main.userInput.gameObject.GetComponentInChildren<InputField>()?.OnSelect(null);
				InputHelper.resetCursorToCenter();
			}
		}
	}
}