using System;
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
	}
}