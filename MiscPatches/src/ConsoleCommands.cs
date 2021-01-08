using System;
using System.Linq;
using System.Text;
using System.Collections;

using UWE;
using UnityEngine;

using Common;
using Common.Reflection;

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

		public void showhead(bool val)
		{
			Player.main.SetHeadVisible(val);
		}

		public void print_mod_console_commands(string modID = "")
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				string fullname = assembly.FullName.ToLower();
				string name = fullname.Replace(".consolecommands", "");

				if (fullname == name || !name.Contains(modID))
					continue;

				foreach (var type in assembly.GetTypes())
				{
					$"<color=yellow>{type.FullName.Replace(".CommandProxy", "")}</color>".onScreen();

					var methods = type.methods().
						Where(method => !method.Name.Contains("<") && method.Name.Contains("OnConsoleCommand_")).
						Select(method => method.Name.Replace("OnConsoleCommand_", "")).
						ToList();

					methods.Sort();
					string.Join(modID == ""? "; ": "\n", methods).onScreen();
				}
			}
		}

		[Command(combineArgs = true, caseSensitive = true)]
		public void subtitles(string message)
		{
#if GAME_SN
			Subtitles.main.Add(message);
#elif GAME_BZ
			Subtitles.Add(message);
#endif
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
			if (PrefabDatabase.TryGetPrefabFilename(prefab, out string prefabPath))
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

		public void dumpprefabcache()
		{
			GameObject.Find("/SMLHelper.PrefabCache")?.dump();
		}

		public void pinprefabcache(bool val = true)
		{
			var prefabRoot = GameObject.Find("/SMLHelper.PrefabCache")?.transform.GetChild(0);

			if (!prefabRoot)
				return;

			StopAllCoroutines();

			if (val)
				StartCoroutine(_pincache());

			IEnumerator _pincache()
			{
				var sb = new StringBuilder();

				while (true)
				{
					sb.Clear();
					sb.AppendLine();

					foreach (Transform prefab in prefabRoot)
						sb.AppendLine(prefab.name);

					sb.ToString().onScreen("prefab cache");
					yield return null;
				}
			}
		}

#pragma warning disable CS0618 // obsolete
		public void game_startnew(GameMode gameMode = GameMode.Creative)
		{
			if (uGUI_MainMenu.main)
				CoroutineHost.StartCoroutine(uGUI_MainMenu.main.StartNewGame(gameMode));
		}
#pragma warning restore CS0618

#if GAME_SN
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
#endif
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
#if GAME_SN
				Targeting.GetTarget(Player.main.gameObject, 10f, out GameObject target, out float num, null);
#elif GAME_BZ
				Targeting.GetTarget(Player.main.gameObject, 10f, out GameObject target, out float num);
#endif
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