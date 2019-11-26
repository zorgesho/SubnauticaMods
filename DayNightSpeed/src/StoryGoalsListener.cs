using System.Collections.Generic;
using UnityEngine;

using Common;
using Common.GameSerialization;

namespace DayNightSpeed
{
	static partial class DayNightSpeedControl
	{
		class StoryGoalsListener: MonoBehaviour
		{
			const float shortGoalDelay = 60f;

			static bool patched = false;
			static StoryGoalsListener instance = null;
			List<string> goals = new List<string>(); // goals with delay less than shortGoalDelay

			const string saveName = "goals";
			SaveLoadHelper slHelper;
			class SaveData { public List<string> goals; }

			void Awake()
			{
				if (instance != null)
				{
					$"StoryGoalsListener already created!".logError();
					Destroy(this);
					return;
				}

				instance = this;

				slHelper = new SaveLoadHelper(null, onSave);
				init();
			}

			void OnDestroy() => instance = null;

			void Update() => slHelper.update();

			public static void load() => instance?.onLoad();

			void onLoad()
			{
				goals = SaveLoad.load<SaveData>(saveName)?.goals ?? new List<string>();
				forcedNormalSpeed = (goals.Count != 0);
			}

			void onSave()
			{
				SaveLoad.save(saveName, new SaveData { goals = goals });
			}

			static void onAddGoal(Story.StoryGoal goal) // postfix for StoryGoalScheduler.Schedule(StoryGoal goal)
			{
				if (goal == null || goal.delay > shortGoalDelay)
					return;

				forcedNormalSpeed = true;

				instance.goals.Add(goal.key);	$"StoryGoalsListener: goal added '{goal.key}'".logDbg();
			}

			static void onRemoveGoal(string key) // postfix for StoryGoal.Execute(string key, GoalType goalType)
			{
				if (instance.goals.RemoveAll(g => g == key) > 0 && instance.goals.Count == 0)
					forcedNormalSpeed = false;
			}

			public static void init()
			{
				if (patched)
					return;

				patched = true;
				HarmonyHelper.patch(typeof(Story.StoryGoalScheduler).method("Schedule"), postfix: typeof(StoryGoalsListener).method(nameof(onAddGoal)));
				HarmonyHelper.patch(typeof(Story.StoryGoal).method("Execute"), postfix: typeof(StoryGoalsListener).method(nameof(onRemoveGoal)));
			}
		}
	}
}