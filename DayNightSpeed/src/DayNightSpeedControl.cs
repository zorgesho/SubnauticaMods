using System.Collections.Generic;
using UnityEngine;

using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	static partial class DayNightSpeedControl
	{
		static bool inited = false;
		static GameObject gameObject;

		// need to force normal speed during most story events
		public static bool forcedNormalSpeed
		{
			get => _forcedNormalSpeed;

			set
			{																											"DayNightCycle.main == null".logDbgError((DayNightCycle.main == null));
				if (_forcedNormalSpeed == value || DayNightCycle.main.IsInSkipTimeMode())
					return;

				_forcedNormalSpeed = value;

				DayNightCycle.main._dayNightSpeed = _forcedNormalSpeed? 1.0f: Main.config.dayNightSpeed;
			}
		}
		static bool _forcedNormalSpeed = false;

		static void initDayNightCycle(DayNightCycle __instance) // postfix for DayNightCycle.Awake
		{
			__instance._dayNightSpeed = Main.config.dayNightSpeed;

			StoryGoalsListener.load(); // need load that after DayNightCycle is created

			// unregistering vanilla daynightspeed console command, replacing it with ours in DayNightSpeedControl
			NotificationCenter.DefaultCenter.RemoveObserver(__instance, "OnConsoleCommand_daynightspeed");
		}

		// for transpilers
		public static float getDayNightSpeedClamped01()
		{																												"DayNightCycle.main == null".logDbgError((DayNightCycle.main == null));
			if (_forcedNormalSpeed)
				return 1.0f;

			float speed = DayNightCycle.main.dayNightSpeed;
			return (speed > float.Epsilon && speed < 1.0f)? speed: 1.0f;
		}

		// called after dayNightSpeed changed via options menu or console
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				if (forcedNormalSpeed)
					return;

				if (DayNightCycle.main != null)
				{
					DayNightCycle.main._dayNightSpeed = Main.config.dayNightSpeed;
					DayNightCycle.main.skipTimeMode = false;
				}
			}
		}

		class DayNightSpeedCommands: PersistentConsoleCommands
		{
			void OnConsoleCommand_daynightspeed(NotificationCenter.Notification n)
			{
				if (n.getArgsCount() > 0)
					DevConsole.SendConsoleCommand($"setcfgvar dns.{nameof(Main.config.dayNightSpeed)} {n.getArg(0)}");

				if (DayNightCycle.main != null)
					$"Day/night speed is {DayNightCycle.main.dayNightSpeed}".onScreen();
			}
		}

		// for debugging
		class DayNightSpeedWatch: MonoBehaviour
		{
			readonly HashSet<string> goals = new HashSet<string>(); // for ignoring duplicates

			void Update()
			{
				if (DayNightCycle.main == null)
					return;

				string clr = forcedNormalSpeed? "<color=#00FF00FF>": "<color=#CCCCCCFF>";
				$"{clr}game:</color>{DayNightCycle.main.dayNightSpeed} <color=#CCCCCCFF>cfg:</color>{Main.config.dayNightSpeed}".onScreen("day/night speed");
				$"{DayNightCycle.main.timePassed:#.###}".onScreen("time passed");
				$"{DayNightCycle.ToGameDateTime(DayNightCycle.main.timePassedAsFloat)}".onScreen("date/time");
#if DEBUG
				if (Main.config.dbgCfg.showGoals && DayNightCycle.main != null) // show current goals
				{
					goals.Clear();
					foreach (var goal in Story.StoryGoalScheduler.main.schedule)
					{
						if (goals.Add(goal.goalKey))
						{
							const string colorCompleted = "<color=#999900CC>", colorNotCompleted = "<color=#FFFF00FF>";
							
							bool completed = Story.StoryGoalManager.main.completedGoals.Contains(goal.goalKey);
							string goalName = (completed? colorCompleted: colorNotCompleted) + goal.goalKey + "</color>";

							$"{(goal.timeExecute - DayNightCycle.main.timePassed):#.###}".onScreen(goalName);
						}
					}
				}
#endif
			}
		}


		public static void init()
		{
			if (inited)
				return;

			inited = true;

			gameObject = UnityHelper.createPersistentGameObject<DayNightSpeedCommands>("DayNightSpeedControl");
			gameObject.AddComponent<StoryGoalsListener>();

			HarmonyHelper.patch(typeof(DayNightCycle).method("Awake"), postfix: typeof(DayNightSpeedControl).method(nameof(initDayNightCycle)));
#if DEBUG
			gameObject.AddComponent<DayNightSpeedWatch>();
#endif
		}
	}

	// helper for transpilers
	static class _dayNightSpeedClamped01
	{
		public static Harmony.CodeInstruction ci
		{
			get => new Harmony.CodeInstruction(System.Reflection.Emit.OpCodes.Call,
				typeof(DayNightSpeedControl).method(nameof(DayNightSpeedControl.getDayNightSpeedClamped01)));
		}
	}
}