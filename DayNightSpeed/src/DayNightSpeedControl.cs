using System.Collections.Generic;
using UnityEngine;

using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	static class DayNightSpeedControl
	{
		static bool inited = false;
		static GameObject gameObject;

		// need to force normal speed during most story events
		public static bool forcedNormalSpeed
		{
			get => _forcedNormalSpeed;

			set
			{
				_forcedNormalSpeed = value;
				DayNightCycle.main._dayNightSpeed = _forcedNormalSpeed? 1.0f: Main.config.dayNightSpeed;
				Main.config.updateValues(DayNightCycle.main._dayNightSpeed);											$"forcedNormalSpeed: {_forcedNormalSpeed}".logDbg();
			}
		}
		static bool _forcedNormalSpeed = false;

		// called after dayNightSpeed changed via options menu or console
		public class SettingChanged: Config.Field.ICustomAction
		{
			public void customAction()
			{
				if (forcedNormalSpeed)
					return;

				Main.config.updateValues(Main.config.dayNightSpeed);

				if (DayNightCycle.main != null)
				{
					DayNightCycle.main._dayNightSpeed = Main.config.dayNightSpeed;
					DayNightCycle.main.skipTimeMode = false;
				}
			}
		}


		static class StoryGoalsListener
		{
			const float shortGoalDelay = 60f;
			static readonly List<string> goals = new List<string>(); // goals with time shorter than shortGoalDelay

			static void onAddGoal(Story.StoryGoal goal) // postfix for StoryGoalScheduler.Schedule(StoryGoal goal)
			{
				if (goal == null || goal.delay > shortGoalDelay)
					return;

				if (goals.Count == 0) // if this a first added goal
					forcedNormalSpeed = true;

				goals.Add(goal.key);	$"StoryGoalsListener: goal added '{goal.key}'".logDbg();
			}

			static void onRemoveGoal(string key) // postfix for StoryGoal.Execute(string key, GoalType goalType)
			{
				if (goals.Remove(key) && goals.Count == 0)
					forcedNormalSpeed = false;
			}

			public static void init()
			{
				HarmonyHelper.patch(typeof(Story.StoryGoalScheduler).method("Schedule"), postfix: typeof(StoryGoalsListener).method(nameof(onAddGoal)));
				HarmonyHelper.patch(typeof(Story.StoryGoal).method("Execute"), postfix: typeof(StoryGoalsListener).method(nameof(onRemoveGoal)));
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
#if DEBUG
				if (Main.config.dbgCfg.showGoals && DayNightCycle.main != null)
				{
					foreach (var goal in Story.StoryGoalScheduler.main.schedule)
						$"{goal.timeExecute - DayNightCycle.main.timePassed}".onScreen("<color=#BBBBFFFF>" + goal.goalKey + "</color>");
				}
#endif
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
			if (inited)
				return;

			inited = true;

			gameObject = UnityHelper.createPersistentGameObject<DayNightSpeedCommands>("DayNightSpeedControl");
#if DEBUG
			gameObject.AddComponent<DayNightSpeedWatch>();
#endif
			StoryGoalsListener.init();
		}
	}
}