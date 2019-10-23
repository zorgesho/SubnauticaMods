using System;

namespace Common.Configuration
{
	partial class Config
	{
		public partial class Field
		{
			// implement this to create custom action when config field is changes
			public interface ICustomAction
			{
				void customAction();
			}

			[AttributeUsage(AttributeTargets.Field)]
			public class CustomActionAttribute: Attribute
			{
				public readonly ICustomAction action;

				public CustomActionAttribute(Type actionType)
				{
					action = Activator.CreateInstance(actionType) as ICustomAction;

					if (action == null)
						$"Field.CustomActionAttribute: '{actionType}' You need to implement ICustomAction in CustomActionType".logError();
				}
			}
		}
	}
}