using System;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class Field
		{
			// implement this to create custom action when config field is changes
			public interface IAction
			{
				void action();
			}

			[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
			public class ActionAttribute: Attribute
			{
				public readonly IAction action;

				public ActionAttribute(Type actionType)
				{
					action = Activator.CreateInstance(actionType) as IAction;
					Debug.assert(action != null, $"Field.ActionAttribute: '{actionType}' You need to implement IAction in ActionType");
				}
			}
		}
	}
}