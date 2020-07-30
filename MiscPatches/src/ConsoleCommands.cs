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