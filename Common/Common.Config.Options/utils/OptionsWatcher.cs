using System.Collections.Generic;
using Harmony;

namespace Common.Configuration.Utils
{
	using Harmony;

	// class for watching enabling/disabling of options menu
	// in case of disabled menu will activate passed actions immediately
	// in case of currently active options menu this class will accumulate actions and activate them all when options is disabled
	// same actions or actions with the same type will not be duplicated
	static class OptionsWatcher
	{
		class ActionComparer: IEqualityComparer<Config.Field.IAction>
		{
			public bool Equals(Config.Field.IAction x, Config.Field.IAction y)
			{
				if (!object.Equals(x, y) && !Equals(x.GetType(), y.GetType()))
					return false;

				// if types are equal we try to compare action parameters
				if ((x as Config.Field.IActionArgs)?.args is object[] argsX && (y as Config.Field.IActionArgs)?.args is object[] argsY)
				{
					if (argsX.Length != argsY.Length)
						return false;

					for (int i = 0; i < argsX.Length; i++)
						if (!Equals(argsX[i], argsY[i]))
							return false;
				}

				return true;
			}

			public int GetHashCode(Config.Field.IAction obj) => obj.GetType().GetHashCode();
		}

		static bool optionsActive = false;
		static readonly HashSet<Config.Field.IAction> postponedActions = new HashSet<Config.Field.IAction>(new ActionComparer());

		public static void processAction(Config.Field.IAction action)
		{
			init();

			if (optionsActive)
				postponedActions.Add(action);
			else
				action.action();
		}

		static bool inited = false;
		static void init()
		{
			if (inited || !(inited = true))
				return;

			HarmonyHelper.patch();
			optionsActive = UnityEngine.Object.FindObjectOfType<uGUI_OptionsPanel>()?.isActiveAndEnabled ?? false;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(uGUI_OptionsPanel), "OnEnable")]
		static void onPanelEnable()
		{														"uGUI_OptionsPanel enabled".logDbg();
			optionsActive = true;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(uGUI_OptionsPanel), "OnDisable")]
		static void onPanelDisable()
		{														"uGUI_OptionsPanel disabled".logDbg();
			optionsActive = false;
			processPostponedActions();
		}

		static void processPostponedActions()
		{
			postponedActions.forEach(a => a.action());
			postponedActions.Clear();
		}
	}
}