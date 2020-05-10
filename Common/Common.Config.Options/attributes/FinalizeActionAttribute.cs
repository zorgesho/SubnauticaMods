using System;
using System.Collections.Generic;

using Harmony;

namespace Common.Configuration
{
	partial class Options
	{
		// action on event of changing field
		// works similar to Field.Action, but in case that options menu is currently open action will be postponed until menu is closed
		// for multiple actions of the same type only one will be activated
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
		public class FinalizeActionAttribute: Config.Field.ActionAttribute
		{
			readonly Type innerActionType;

			public FinalizeActionAttribute(Type actionType): base(typeof(ProxyAction)) => innerActionType = actionType;

			public override Config.Field.IAction action
			{
				get
				{
					if (_action == null)
						(base.action as ProxyAction).init(createAction(innerActionType));

					Debug.assert(_action != null);
					return _action;
				}
			}


			class ProxyAction: Config.Field.IAction
			{
				Config.Field.IAction innerAction;

				public void init(Config.Field.IAction innerAction) => this.innerAction = innerAction;

				public void action() => OptionsWatcher.processAction(innerAction);
			}


			// class for watching enabling/disabling of options menu
			// in case of disabled menu will activate passed actions immediately
			// in case of currently active options menu this class will accumulate actions and activate them all when options is disabled
			// same actions or actions with the same type will not be duplicated
			static class OptionsWatcher
			{
				class ActionComparer: IEqualityComparer<Config.Field.IAction>
				{
					public bool Equals(Config.Field.IAction x, Config.Field.IAction y) =>
						object.Equals(x, y)? true: Equals(x.GetType(), y.GetType());

					public int GetHashCode(Config.Field.IAction obj) => obj.GetType().GetHashCode();
				}

				static readonly HashSet<Config.Field.IAction> postponedActions = new HashSet<Config.Field.IAction>(new ActionComparer());

				static bool optionsActive = false;

				static bool inited = false;
				static void init()
				{
					if (inited || !(inited = true))
						return;

					HarmonyHelper.patch();
					optionsActive = UnityEngine.Object.FindObjectOfType<uGUI_OptionsPanel>()?.isActiveAndEnabled ?? false;
				}

				[HarmonyPatch(typeof(uGUI_OptionsPanel), "OnEnable")] [HarmonyPostfix]
				static void onPanelEnable()
				{														"uGUI_OptionsPanel enabled".logDbg();
					optionsActive = true;
				}

				[HarmonyPatch(typeof(uGUI_OptionsPanel), "OnDisable")] [HarmonyPostfix]
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

				public static void processAction(Config.Field.IAction action)
				{
					init();

					if (optionsActive)
						postponedActions.Add(action);
					else
						action.action();
				}
			}
		}
	}
}