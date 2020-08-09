using System;

using UWE;
using UnityEngine;

using Common;

namespace MiscPatches
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		public void printinventory()
		{
			if (!Inventory.main)
				return;

			"Inventory items:".log();
			Inventory.main.container.ForEach(item => $"item: {item.item.GetTechName()}".onScreen().log());
		}

		[Command(combineArgs = true, caseSensitive = true)]
		public void subtitles(string message)
		{
			Subtitles.main.Add(message);
		}

		public void vehiclehealth(float healthPercent)
		{
			if (Player.main?.GetVehicle()?.GetComponent<LiveMixin>() is LiveMixin liveMixin)
				liveMixin.health = liveMixin.maxHealth * healthPercent;
		}

		public void lootreroll()
		{
			LargeWorldStreamer.main?.ForceUnloadAll();
		}

		public void gc()
		{
			GC.Collect();
		}

		public void spawnresource(string prefab)
		{
			// if parameter is prefabID
			if (UWE.PrefabDatabase.TryGetPrefabFilename(prefab, out string prefabPath))
				prefab = prefabPath;

			Utils.CreatePrefab(Resources.Load<GameObject>(prefab));
		}

		public void setmovetarget()
		{
			if (Main.config.objectsMoveStep > 0f)
				$"Target: {ObjectMover.findTarget()?.name ?? "null"}".onScreen();
		}

		public void moveobject(float dx, float dy, float dz)
		{
			if (Main.config.objectsMoveStep == 0f)
				return;

			var offset = new Vector3(dx, dy, dz) * Main.config.objectsMoveStep;

			if (!ObjectMover.moveObject(offset))
				"No target is selected!".onScreen();
		}

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
				CoroutineHost.StartCoroutine(uGUI_MainMenu.main.LoadGameAsync(slotToLoad, gameinfoToLoad.changeSet, gameinfoToLoad.gameMode));
		}

		public void game_quit(bool quitToDesktop = false)
		{
			if (uGUI_MainMenu.main && quitToDesktop)
				Application.Quit();
			else
				IngameMenu.main?.QuitGame(quitToDesktop);
		}

		public void initial_equipment(TechType techType = default, int count = 1)
		{
			if (techType == default)
				Main.config.dbg.initialEquipment.Clear();
			else
				Main.config.dbg.initialEquipment[techType] = count;

			Main.config.save();
		}

		static class ObjectMover
		{
			static GameObject moveTarget;

			const float maxTime = 20f;
			static float lastTargetActionTime;

			public static GameObject findTarget()
			{
				Targeting.GetTarget(Player.main.gameObject, 10f, out GameObject target, out float num, null);
				moveTarget = target?.GetComponentInParent<Constructable>()?.gameObject;
				lastTargetActionTime = Time.time;

				return moveTarget;
			}

			public static bool moveObject(Vector3 offset)
			{
				if (Time.time - lastTargetActionTime > maxTime)
					moveTarget = null;

				if (!moveTarget)
					return false;

				moveTarget.transform.localPosition += moveTarget.transform.localRotation * offset;
				lastTargetActionTime = Time.time;

				return true;
			}
		}
	}
}