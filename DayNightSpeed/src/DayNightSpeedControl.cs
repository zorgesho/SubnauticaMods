using UnityEngine;
using Common;

using Common.Configuration;

namespace DayNightSpeed
{
	static class DayNightSpeedControl
	{
#if DEBUG
		static GameObject watcherGO = null;
#endif
		static GameObject commandsGO = null;

		// called after dayNightSpeed changed via options menu or console
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				Main.config.updateValues();

				if (DayNightCycle.main != null)
				{
					DayNightCycle.main._dayNightSpeed = Main.config.dayNightSpeed;
					DayNightCycle.main.skipTimeMode = false;
				}
			}
		}

		class DayNightSpeedWatch: MonoBehaviour
		{
			void Update()
			{
				if (DayNightCycle.main == null)
					return;

				$"<color=#CCCCCCFF>game:</color>{DayNightCycle.main.dayNightSpeed} <color=#CCCCCCFF>cfg:</color>{Main.config.dayNightSpeed}".onScreen("dayNightSpeed");
				$"{DayNightCycle.main.timePassed}".onScreen("time passed");
				$"{DayNightCycle.ToGameDateTime(DayNightCycle.main.timePassedAsFloat)}".onScreen("date/time");
			}
		}

		class DayNightSpeedCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_daynightspeed(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() > 0)
					DevConsole.SendConsoleCommand($"setcfgvar {nameof(Main.config.dayNightSpeed)} {n.getArg(0)}");

				if (DayNightCycle.main != null)
					$"Day/night speed is {DayNightCycle.main.dayNightSpeed}".onScreen();
			}

#if DEBUG
			void OnConsoleCommand_dischargebatts(NotificationCenter.Notification _)
			{
				if (Inventory.main == null)
					return;

				foreach (InventoryItem item in Inventory.main.container)
				{
					Battery battery = item.item.gameObject.GetComponent<Battery>();

					if (battery != null)
						battery.charge = 0;
				}
			}
#endif
		}

		public static void init()
		{
			if (commandsGO == null)
				commandsGO = PersistentConsoleCommands.createGameObject<DayNightSpeedCommands>("DayNightSpeedConsoleCommands");
#if DEBUG
			if (watcherGO == null)
				watcherGO = UnityHelper.createPersistentGameObject<DayNightSpeedWatch>("DayNightSpeedWatcher");
#endif
		}
	}
}