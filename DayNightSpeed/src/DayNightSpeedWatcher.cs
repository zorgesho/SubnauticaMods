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

		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				if (DayNightCycle.main == null)
					return;

				DayNightCycle.main._dayNightSpeed = Main.config.dayNightSpeed;
				DayNightCycle.main.skipTimeMode = false;
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