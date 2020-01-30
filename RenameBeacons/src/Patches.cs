using System.Text;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;

namespace RenameBeacons
{
	[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
	static class TooltipFactory_ItemActions_Beacon_Patch
	{
		static void Postfix(StringBuilder sb, InventoryItem item)
		{
			if (item.item.GetTechType() == TechType.Beacon)
				TooltipFactory.WriteAction(sb, Strings.Mouse.middleButton, L10n.str(L10n.ids_rename));
		}
	}

	[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
	static class TooltipFactory_ItemCommons_Beacon_Patch
	{
		static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
		{
			if (techType == TechType.Beacon)
				TooltipFactory.WriteDescription(sb, $"{L10n.str(L10n.ids_name)}: \"{obj.GetComponent<Beacon>().label}\"");
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
			if (item.item.GetTechType() != TechType.Beacon || button != 2)
				return;

			Beacon beacon = item.item.GetComponent<Beacon>();
			BeaconRenamer.init(beacon);

			// for receiving events from mouse when opened from inventory
			uGUI.main.userInput.gameObject.ensureComponent<uGUI_GraphicRaycaster>().guiCameraSpace = true;

			uGUI.main.userInput.RequestString(	Language.main.Get("BeaconLabel"),
												Language.main.Get("BeaconSubmit"),
												beacon.beaconLabel.labelName, 25,
												new uGUI_UserInput.UserInputCallback(BeaconRenamer.setLabel));

			uGUI.main.userInput.gameObject.GetComponentInChildren<InputField>()?.OnSelect(null); // set focus to input field
			Cursor.lockState = CursorLockMode.Locked; // for resetting cursor to the center of the screen (it's unlocks on the next frame in UWE.Utils.UpdateCusorLockState)
		}
	}
}