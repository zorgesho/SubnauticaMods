using System;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class Field
		{
			// implement this to create custom action when value of config field is changed
			public interface IAction
			{
				void action();
			}

			[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
			public class ActionAttribute: Attribute
			{
				readonly Type actionType;

				public ActionAttribute(Type actionType) => this.actionType = actionType;

				public virtual IAction action => _action ??= createAction(actionType);
				protected IAction _action;

				protected IAction createAction(Type _actionType)
				{																									$"Field.ActionAttribute: creating action object ({_actionType})".logDbg();
					var action = Activator.CreateInstance(_actionType) as IAction;
					Debug.assert(action != null, $"Field.ActionAttribute: '{_actionType}' You need to implement IAction in ActionType");

					return action;
				}
			}
		}
	}
}