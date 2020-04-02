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

			[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
			public class ActionAttribute: Attribute
			{
				public readonly IAction action;

				protected IAction createAction(Type actionType)
				{
					IAction action = Activator.CreateInstance(actionType) as IAction;
					Debug.assert(action != null, $"Field.ActionAttribute: '{actionType}' You need to implement IAction in ActionType");

					return action;
				}

				public ActionAttribute(Type actionType) => action = createAction(actionType); // TODO move init to property
			}
		}
	}
}