using System.Text;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Harmony;

namespace UITweaks
{
	static class BeaconRenamer
	{
		static void renameBeacon(Beacon beacon)
		{
			beacon.beaconLabel.SetLabel(beacon.label);

			uGUI_UserInput input = uGUI.main.userInput;

			// for receiving events from mouse when opened from inventory
			input.gameObject.ensureComponent<uGUI_GraphicRaycaster>().guiCameraSpace = true;

			var callback = new uGUI_UserInput.UserInputCallback(label =>
			{
				beacon.label = label;
				beacon.beaconLabel.SetLabel(label);
			});

			input.RequestString(Language.main.Get("BeaconLabel"), Language.main.Get("BeaconSubmit"), beacon.beaconLabel.labelName, 25, callback);

			input.gameObject.GetComponentInChildren<InputField>()?.OnSelect(null); // set focus to input field
			Cursor.lockState = CursorLockMode.Locked; // for resetting cursor to the center of the screen (it's unlocks on the next frame in UWE.Utils.UpdateCusorLockState)
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => Main.config.renameBeacons;

			[HarmonyPostfix, HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
			static void TooltipFactory_ItemActions_Postfix(StringBuilder sb, InventoryItem item)
			{
				if (item.item.GetTechType() == TechType.Beacon)
					TooltipFactory.WriteAction(sb, Strings.Mouse.middleButton, L10n.str(L10n.ids_beaconRename));
			}

			[HarmonyPostfix, HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
			static void TooltipFactory_ItemCommons_Postfix(StringBuilder sb, TechType techType, GameObject obj)
			{
				if (techType == TechType.Beacon)
					TooltipFactory.WriteDescription(sb, $"{L10n.str(L10n.ids_beaconName)}: \"{obj.GetComponent<Beacon>().label}\"");
			}

			[HarmonyPostfix, HarmonyPatch(typeof(Beacon), "OnPickedUp")]
			static void Beacon_OnPickedUp_Postfix(Beacon __instance)
			{
				__instance.label = __instance.beaconLabel.GetLabel();
			}

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
			static void uGUIInventoryTab_OnPointerClick_Postfix(InventoryItem item, int button)
			{
				if (item.item.GetTechType() == TechType.Beacon && button == 2)
					renameBeacon(item.item.GetComponent<Beacon>());
			}
		}
	}
}