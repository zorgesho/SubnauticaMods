using System;

namespace Common.Configuration
{
	using Utils;

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
		}
	}
}