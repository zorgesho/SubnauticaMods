using System;

namespace Common.Config
{
	partial class Config
	{
		// implement this to create custom action when config field is changes
		public interface IFieldCustomAction
		{
			void fieldCustomAction();
		}

		
		[AttributeUsage(AttributeTargets.Field)]
		public class FieldCustomActionAttribute: Attribute
		{
			public readonly IFieldCustomAction action = null;

			public FieldCustomActionAttribute(Type actionType)
			{
				action = Activator.CreateInstance(actionType) as IFieldCustomAction;
				
				if (action == null)
					$"FieldCustomActionAttribute: '{actionType}' You need to implement IFieldCustomAction in CustomActionType".logError();
			}
		}
	}
}