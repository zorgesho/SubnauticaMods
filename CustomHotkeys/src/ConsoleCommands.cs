using System.Collections;

using UWE;
using UnityEngine;

using Common;
using Common.Reflection;
using Common.Configuration;

namespace CustomHotkeys
{
	using Math = System.Math;

	class ConsoleCommands: PersistentConsoleCommands
	{
		#region dev tools & debug commands

		static void toggleComponent<T>() where T: MonoBehaviour => FindObjectsOfType<T>().ForEach(cmp => cmp.enabled = !cmp.enabled);

		public void devtools_toggleterrain()	=> toggleComponent<TerrainDebugGUI>();
		public void devtools_togglegraphics()	=> toggleComponent<GraphicsDebugGUI>();
		public void devtools_toggleframegraph()	=> toggleComponent<FrameTimeOverlay>();

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
			Options.Utils.open();
		}

		public void wait(float secs)
		{
			HotkeyHelper.wait(secs);
		}

		[Command(combineArgs = true)]
		public void addhotkey(string command)
		{
			Main.hkConfig.addHotkey(new HKConfig.Hotkey() { command = command, label = "", mode = HKConfig.Hotkey.Mode.Press });

			Options.Utils.open();
			StartCoroutine(_scroll());

			static IEnumerator _scroll()
			{
				yield return null;
				Options.Utils.scrollToShowOption(-1);
			}
		}

		public void hktools_mark_unassigned_as_hidden(bool val = true)
		{
			foreach (var hotkey in Main.hkConfig.hotkeys)
				if (hotkey.key == default)
					hotkey.hidden = val;

			Main.hkConfig.save();
		}

		public void hktools_add_default_fields()
		{
			foreach (var hotkey in Main.hkConfig.hotkeys)
			{
				hotkey.mode ??= default;
				hotkey.label ??= "";
				hotkey.hidden ??= false;
			}

			Main.hkConfig.save();
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
#if GAME_SN // doesn't needed for BZ
		public void autoforward(bool? enabled)
		{
			if (enabled == null)
				GameInput_AutoForward_Patch.toggleAutoForward();
			else
				GameInput_AutoForward_Patch.setAutoForward((bool)enabled);
		}
#endif
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
#if GAME_SN
					Inventory.main.UseItem(item);
#elif GAME_BZ
					Inventory.main.ExecuteItemAction(item, 0);
#endif
					break;
				}
			}
		}

		public void vehicle_enter(float distance = 6f)
		{
			if (getProperVehicle(distance) is Vehicle vehicle)
			{
				vehicle.EnterVehicle(Player.main, true, true);
			}
#if GAME_BZ
			else if (getProperSeaTruck(distance) is SeaTruckSegment truck)
			{
				truck.motor.StartPiloting();
				truck.seatruckanimation.currentAnimation = SeaTruckAnimation.Animation.EnterPilot;
				truck.Enter(Player.main);
				Utils.PlayFMODAsset(truck.enterSound, Player.main.transform);
			}
#endif
		}

		public void vehicle_upgrades()
		{
			MonoBehaviour vehicle = getProperVehicle(4f);
#if GAME_BZ
			vehicle ??= getProperSeaTruck(4f);
#endif
			vehicle?.GetComponentInChildren<VehicleUpgradeConsoleInput>().OnHandClick(null);
		}
#if GAME_BZ
		public void seatruck_forcedexit()
		{
			SeaTruckForcedExit.exitFrom(getPilotedSeaTruck()?.motor);
		}

		public void seatruck_dropmodules()
		{
			getPilotedSeaTruck()?.Detach();
		}
#endif
		#endregion

		#region game* commands
#pragma warning disable CS0618 // obsolete
		public void game_startnew(GameMode gameMode = GameMode.Creative)
		{
			if (uGUI_MainMenu.main)
				CoroutineHost.StartCoroutine(uGUI_MainMenu.main.StartNewGame(gameMode));
		}
#pragma warning restore CS0618

		public void game_load(int slotID = -1)
		{
			if (!uGUI_MainMenu.main)
				return;

			string slotToLoad = null;
			SaveLoadManager.GameInfo gameinfoToLoad = null;

			if (slotID == -1) // loading most recent save
			{
				foreach (var slot in SaveLoadManager.main.GetActiveSlotNames())
				{
					var gameinfo = SaveLoadManager.main.GetGameInfo(slot);
					gameinfoToLoad ??= gameinfo;

					if (gameinfoToLoad.dateTicks < gameinfo.dateTicks)
					{
						slotToLoad = slot;
						gameinfoToLoad = gameinfo;
					}
				}
			}
			else
			{
				slotToLoad = $"slot{slotID:D4}";
				gameinfoToLoad = SaveLoadManager.main.GetGameInfo(slotToLoad);
			}

			if (gameinfoToLoad != null)
#if GAME_SN
				CoroutineHost.StartCoroutine(uGUI_MainMenu.main.LoadGameAsync(slotToLoad, gameinfoToLoad.changeSet, gameinfoToLoad.gameMode));
#elif GAME_BZ
				CoroutineHost.StartCoroutine(uGUI_MainMenu.main.LoadGameAsync(slotToLoad, "", gameinfoToLoad.changeSet, gameinfoToLoad.gameMode, 2));
#endif
		}

		public void game_save()
		{
			CoroutineHost.StartCoroutine(IngameMenu.main?.SaveGameAsync());
		}

		public void game_quit(bool quitToDesktop = false)
		{
			if (uGUI_MainMenu.main && quitToDesktop)
				Application.Quit();
			else
				IngameMenu.main?.QuitGame(quitToDesktop);
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
			if (GameUtils.findNearestToPlayer<Vehicle>(out float distSq) is Vehicle vehicle)
				return distSq < maxDistance * maxDistance? vehicle: null;

			return null;
		}

#if GAME_BZ
		static SeaTruckSegment getProperSeaTruck(float maxDistance)
		{
			if (Player.main?.currentInterior != null)
				return null;

			if (findNearestSeaTruckCabin(maxDistance) is SeaTruckSegment truck && truck.CanEnter())
				return truck.GetComponent<Dockable>()?.isDocked == true? null: truck;

			return null;
		}

		static SeaTruckSegment findNearestSeaTruckCabin(float maxDistance)
		{
			if (GameUtils.findNearestToPlayer<SeaTruckSegment>(out float distSq, sts => sts.isMainCab) is SeaTruckSegment truck)
				return distSq < maxDistance * maxDistance? truck: null;

			return null;
		}

		static SeaTruckSegment getPilotedSeaTruck()
		{
			return Player.main?.inSeatruckPilotingChair != true? null: Player.main.GetComponentInParent<SeaTruckSegment>();
		}
#endif
		#endregion
	}
}