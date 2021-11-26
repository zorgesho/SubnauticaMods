using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Story;

using Common;
using Common.Harmony;
using Common.GameSerialization;

#if DEBUG
using System.Linq;
#endif

namespace DayNightSpeed
{
	static partial class DayNightSpeedControl
	{
		[PatchClass]
		class StoryGoalsListener: MonoBehaviour
		{
			const float shortGoalDelay = 60f;
			const string saveName = "goals";

			static StoryGoalsListener instance = null;

			List<string> goals = new(); // goals with delay less than shortGoalDelay
			void updateForcedMode() => forcedNormalSpeed = (goals.Count != 0);

			SaveLoadHelper slHelper;
			class SaveData { public List<string> goals; }

			void Awake()
			{
				if (instance != null)
				{
					"StoryGoalsListener already created!".logError();
					Destroy(this);
					return;
				}

				instance = this;
				slHelper = new SaveLoadHelper(null, onSave);
			}

			void OnDestroy() => instance = null;

			void Update()
			{
				slHelper.update();
#if GAME_BZ
				cleanUpGoals();
#endif
#if DEBUG
				if (goals.Count > 0 && Main.config.dbgCfg.enabled && Main.config.dbgCfg.showGoals)
					string.Join(" ", goals.Select(goal => $"'{goal}'")).onScreen("goals listener");
#endif
			}

#if GAME_BZ // in BZ, goals often ignoring 'timeExecute', so we need to remove them here
			void cleanUpGoals()
			{
				if (!StoryGoalScheduler.main || !DayNightCycle.main)
					return;

				static bool _isGoalExpired(string goal) =>
					StoryGoalScheduler.main.GetScheduledGoal(goal)?.timeExecute < DayNightCycle.main.timePassed + float.Epsilon;

				goals.RemoveAll(goal => goal == "" || _isGoalExpired(goal));
				updateForcedMode();
			}
#endif
			public static void load() => instance?.onLoad();

			void onLoad()
			{
				goals = SaveLoad.load<SaveData>(saveName)?.goals ?? new List<string>();
				updateForcedMode();
			}

			void onSave()
			{
				SaveLoad.save(saveName, new SaveData { goals = goals });
			}

			public static bool isGoalCompleted(string goalKey) => StoryGoalManager.main.completedGoals.Contains(goalKey);

			static bool shouldIgnoreGoal(StoryGoal goal) =>
				goal == null || goal.key.isNullOrEmpty() || goal.delay == 0f || goal.delay > shortGoalDelay;

			[HarmonyPostfix, HarmonyPatch(typeof(StoryGoalScheduler), "Schedule")]
			static void onAddGoal(StoryGoal goal)
			{
				if (shouldIgnoreGoal(goal) || isGoalCompleted(goal.key))
					return;

				forcedNormalSpeed = true;
				instance.goals.Add(goal.key);											$"StoryGoalsListener: goal added '{goal.key}'".logDbg();
			}

			[HarmonyPostfix, HarmonyPatch(typeof(StoryGoal), "Execute")]
			static void onRemoveGoal(string key)
			{
				instance.goals.RemoveAll(g => g == key);
				instance.updateForcedMode();
			}
		}
	}
}