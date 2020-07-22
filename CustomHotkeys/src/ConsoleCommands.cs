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

		public void devtools_toggleterrain()	=> toggleComponent<TerrainDebugGUI>();
		public void devtools_togglegraphics()	=> toggleComponent<GraphicsDebugGUI>();
		public void devtools_toggleframegraph()	=> toggleComponent<UWE.FrameTimeOverlay>();

		public void devtools_hidegui(GUIController.HidePhase? hidePhase)
		{
			if (!GUIController.main)
				return;

			int phase = (int)(hidePhase ?? GUIController.main.hidePhase + 1);
			phase %= (int)GUIController.HidePhase.All + 1;

			GUIController.SetHidePhase(GUIController.main.hidePhase = (GUIController.HidePhase)phase);
		}

		public void devtools_wireframe(bool? enabled)
		{
			GL.wireframe = enabled ?? !GL.wireframe;
		}

		Vector3? lastPreWarpPos;

		public void warpmem(float? x, float y = 0f, float z = 0f)
		{
			static void _warp(Vector3 pos)
			{
				Player.main.SetPosition(pos);
				Player.main.OnPlayerPositionCheat();
			}

			if (x != null)
			{
				lastPreWarpPos = Player.main.transform.position;
				_warp(new Vector3((float)x, y, z));
			}
			else
			{
				if (lastPreWarpPos != null)
					_warp((Vector3)lastPreWarpPos);
			}
		}
#if DEBUG
		public void logassoc(string ext)
		{
			WinApi.logAccos(ext);
		}
#endif
		#endregion

		#region misc commands
		public void setwindowpos(int x, int y)
		{
			WinApi.setWindowPos(x, y);
		}

		public void setresolution(int width, int height, bool fullscreen = true)
		{
			DisplayManager.SetResolution(Math.Max(640, width), Math.Max(480, height), fullscreen);
		}

		public void fov(float fov)
		{
			SNCameraRoot.main?.SetFov(fov);
		}

		[Command(caseSensitive = true, combineArgs = true)]
		public void showmessage(string message)
		{
			message.onScreen();
		}

		public void clearmessages()
		{
			GameUtils.clearScreenMessages();
		}

		public void showmodoptions()
		{
			Options.open();
		}

		public void wait(float secs)
		{
			HotkeyHelper.wait(secs);
		}

		[Command(combineArgs = true)]
		public void addhotkey(string command)
		{
			Main.hkConfig.addHotkey(new HKConfig.Hotkey() { command = command, label = "", mode = HKConfig.Hotkey.Mode.Press });

			Options.open();
			StartCoroutine(_scroll());

			static IEnumerator _scroll()
			{
				yield return null;
				Options.scrollToShowOption(-1);
			}
		}

		public void lastcommand(int indexFromEnd = 0)
		{
			var history = DevConsole.instance.history;
			if (history.Count == 0)
				return;

			int index = Math.Max(0, history.Count - Math.Abs(indexFromEnd) - 1);
			if (history[index].Trim() != nameof(lastcommand))
				DevConsole.SendConsoleCommand(history[index]);
		}
		#endregion

		#region gameplay commands
		public void autoforward(bool? enabled)
		{
			if (enabled == null)
				GameInput_AutoForward_Patch.toggleAutoForward();
			else
				GameInput_AutoForward_Patch.setAutoForward((bool)enabled);
		}

		public void bindslot(int slotID, TechType? techType)
		{
			if (!Inventory.main)
				return;

			if (techType == null)
			{
				Inventory.main.quickSlots.Unbind(slotID);
				return;
			}

			if (techType == TechType.None || Inventory.main.quickSlots.binding[slotID]?.item.GetTechType() == techType)
				return;

			if (getItemFromInventory((TechType)techType) is InventoryItem item)
				Inventory.main.quickSlots.Bind(slotID, item);
		}

		public void equipslot(int slotID = -1)
		{
			if (slotID != -1)
				Inventory.main?.quickSlots.SlotKeyDown(slotID);
			else
				Inventory.main?.quickSlots.Deselect();
		}

		public void useitem(Hashtable data)
		{
			if (!Inventory.main)
				return;

			for (int i = 0; i < data.Count; i++)
			{
				var techType = data[i].convert<TechType>();
				if (techType != default && getItemFromInventory(techType) is InventoryItem item)
				{
					Inventory.main.UseItem(item);
					break;
				}
			}
		}

		public void vehicle_enter(float distance = 6f)
		{
			getProperVehicle(distance)?.EnterVehicle(Player.main, true, true);
		}

		public void vehicle_upgrades()
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