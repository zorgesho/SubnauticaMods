using System;
using System.Collections;

using UnityEngine;

using Common;
using Common.Reflection;
using Common.Configuration;

namespace CustomHotkeys
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		#region dev tools & debug commands

		static void toggleComponent<T>() where T: MonoBehaviour => FindObjectsOfType<T>().ForEach(cmp => cmp.enabled = !cmp.enabled);

		void OnConsoleCommand_devtools_toggleterrain(NotificationCenter.Notification _)	   => toggleComponent<TerrainDebugGUI>();
		void OnConsoleCommand_devtools_togglegraphics(NotificationCenter.Notification _)   => toggleComponent<GraphicsDebugGUI>();
		void OnConsoleCommand_devtools_toggleframegraph(NotificationCenter.Notification _) => toggleComponent<UWE.FrameTimeOverlay>();

		void OnConsoleCommand_devtools_hidegui(NotificationCenter.Notification n)
		{
			if (!GUIController.main)
				return;

			int phase = n.getArgCount() > 0? n.getArg<GUIController.HidePhase>(0).cast<int>(): (int)GUIController.main.hidePhase + 1;
			phase %= (int)GUIController.HidePhase.All + 1;

			GUIController.SetHidePhase(GUIController.main.hidePhase = (GUIController.HidePhase)phase);
		}
#if DEBUG
		void OnConsoleCommand_logassoc(NotificationCenter.Notification n)
		{
			if (n.getArgCount() == 1)
				WinApi.logAccos(n.getArg(0));
		}
#endif
		#endregion

		#region misc commands
		void OnConsoleCommand_setwindowpos(NotificationCenter.Notification n)
		{
			WinApi.setWindowPos(n.getArg<int>(0), n.getArg<int>(1));
		}

		void OnConsoleCommand_setresolution(NotificationCenter.Notification n)
		{
			if (n.getArgCount() > 1)
			{
				DisplayManager.SetResolution(Math.Max(640, n.getArg<int>(0)),
											 Math.Max(480, n.getArg<int>(1)),
											 n.getArgCount() == 3? n.getArg<bool>(2): true);
			}
		}

		void OnConsoleCommand_fov(NotificationCenter.Notification n)
		{
			if (n.getArgCount() == 1)
				SNCameraRoot.main?.SetFov(n.getArg<float>(0));
		}

		[CommandData(caseSensitive = true, combineArgs = true)]
		void OnConsoleCommand_showmessage(NotificationCenter.Notification n)
		{
			n.getArg(0)?.onScreen();
		}

		void OnConsoleCommand_clearmessages(NotificationCenter.Notification _)
		{
			GameUtils.clearScreenMessages();
		}

		void OnConsoleCommand_showmodoptions(NotificationCenter.Notification _)
		{
			Options.open();
		}

		void OnConsoleCommand_wait(NotificationCenter.Notification n)
		{
			HotkeyHelper.wait(n.getArg<float>(0));
		}

		[CommandData(combineArgs = true)]
		void OnConsoleCommand_addhotkey(NotificationCenter.Notification n)
		{
			if (n.getArgCount() == 0)
				return;

			Main.hkConfig.addHotkey(new HKConfig.Hotkey() { command = n.getArg(0), label = "", mode = HKConfig.Hotkey.Mode.Press });

			Options.open();
			StartCoroutine(_scroll());

			static IEnumerator _scroll()
			{
				yield return null;
				Options.scrollToShowOption(-1);
			}
		}

		void OnConsoleCommand_lastcommand(NotificationCenter.Notification n)
		{
			var history = DevConsole.instance.history;
			if (history.Count == 0)
				return;

			int index = Math.Max(0, history.Count - Math.Abs(n.getArg<int>(0)) - 1);
			if (history[index].Trim() != "lastcommand")
				DevConsole.SendConsoleCommand(history[index]);
		}
		#endregion

		#region gameplay commands
		void OnConsoleCommand_autoforward(NotificationCenter.Notification n)
		{
			if (n.getArgCount() == 0)
				GameInput_AutoForward_Patch.toggleAutoForward();
			else
				GameInput_AutoForward_Patch.setAutoForward(n.getArg<bool>(0));
		}

		void OnConsoleCommand_bindslot(NotificationCenter.Notification n)
		{
			if (!Inventory.main || n.getArgCount() == 0)
				return;

			int slotID = n.getArg<int>(0);

			if (n.getArgCount() == 1)
			{
				Inventory.main.quickSlots.Unbind(slotID);
				return;
			}

			TechType techType = n.getArg<TechType>(1);

			if (techType == default || Inventory.main.quickSlots.binding[slotID]?.item.GetTechType() == techType)
				return;

			if (getItemFromInventory(techType) is InventoryItem item)
				Inventory.main.quickSlots.Bind(slotID, item);
		}

		void OnConsoleCommand_equipslot(NotificationCenter.Notification n)
		{
			if (n.getArgCount() == 1)
				Inventory.main?.quickSlots.SlotKeyDown(n.getArg<int>(0));
			else
				Inventory.main?.quickSlots.Deselect();
		}

		void OnConsoleCommand_useitem(NotificationCenter.Notification n)
		{
			if (!Inventory.main || n.getArgCount() == 0)
				return;

			for (int i = 0; i < n.getArgCount(); i++)
			{
				TechType techType = n.getArg<TechType>(i);

				if (techType != default && getItemFromInventory(techType) is InventoryItem item)
				{
					Inventory.main.UseItem(item);
					break;
				}
			}
		}

		void OnConsoleCommand_vehicle_enter(NotificationCenter.Notification n)
		{
			getProperVehicle(n.getArgCount() == 1? n.getArg<float>(0): 6f)?.EnterVehicle(Player.main, true, true);
		}

		void OnConsoleCommand_vehicle_upgrades(NotificationCenter.Notification _)
		{
			getProperVehicle(4f)?.GetComponentInChildren<VehicleUpgradeConsoleInput>().OnHandClick(null);
		}
		#endregion

		#region utils
		static InventoryItem getItemFromInventory(TechType techType)
		{
			if (!Inventory.main)
				return null;

			var items = Inventory.main.container.GetItems(techType);
			return (items?.Count > 0)? items[0]: null;
		}

		static Vehicle getProperVehicle(float maxDistance)
		{
			if (!Player.main || Player.main.GetVehicle())
				return null;

			return (findNearestVehicle(maxDistance) is Vehicle vehicle && !vehicle.docked)? vehicle: null;
		}

		static Vehicle findNearestVehicle(float maxDistance)
		{
			if (UnityHelper.findNearestToPlayer<Vehicle>(out float distSq) is Vehicle vehicle)
				return distSq < maxDistance * maxDistance? vehicle: null;

			return null;
		}
		#endregion
	}
}