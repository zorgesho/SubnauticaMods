using System;

namespace Common.Configuration
{
	partial class Options
	{
		// option can be hided (either by option ID or by separate groupID for multiple options)
		[AttributeUsage(AttributeTargets.Field)]
		public class HideableAttribute: Attribute
		{
			public readonly string groupID;
			public readonly Components.Hider.IVisibilityChecker visChecker;

			public HideableAttribute(Type visCheckerType, string _groupID = null)
			{
				groupID = _groupID;

				visChecker = Activator.CreateInstance(visCheckerType) as Components.Hider.IVisibilityChecker;
				Debug.assert(visChecker != null, $"Options.HideableAttribute: '{visCheckerType}' You need to implement IVisibilityChecker in visCheckerType");
			}
		}
	}
}