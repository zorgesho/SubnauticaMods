using UnityEngine;
using Common;

namespace MiscPatches
{
	static class ConsoleCommands
	{
#pragma warning disable IDE0052
		static GameObject go = null;
#pragma warning restore
		class Commands: PersistentConsoleCommands
		{
			void OnConsoleCommand_printinventory(NotificationCenter.Notification _)
			{
				if (!Inventory.main)
					return;

				"Inventory items:".log();
				Inventory.main.container.ForEach(item => $"item: {item.item.GetTechName()}".onScreen().log());
			}

			void OnConsoleCommand_subtitles(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() > 0)
					Subtitles.main.Add(n.getArg(0) as string);
			}

			void OnConsoleCommand_vehiclehealth(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() > 0 && Player.main?.GetVehicle()?.GetComponent<LiveMixin>() is LiveMixin liveMixin)
					liveMixin.health = liveMixin.maxHealth * (n.getArg(0).toFloat());
			}

			void OnConsoleCommand_lootreroll(NotificationCenter.Notification _)
			{
				LargeWorldStreamer.main?.ForceUnloadAll();
			}

			void OnConsoleCommand_spawnresource(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() > 0)
					Utils.CreatePrefab(Resources.Load<GameObject>(n.getArg(0) as string));
			}
		}

		public static void init()
		{
			go ??= PersistentConsoleCommands.createGameObject<Commands>("MiscPatches.ConsoleCommands");
		}
	}
}