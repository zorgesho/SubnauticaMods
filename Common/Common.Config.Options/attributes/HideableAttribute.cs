using System;

namespace Common.Configuration
{
	partial class Options
	{
		// option can be hided (either by option ID or by separate groupID for multiple options)
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
		public class HideableAttribute: Attribute
		{
			readonly Type visCheckerType;
			public readonly string groupID;

			public HideableAttribute(Type visCheckerType, string groupID = null)
			{
				this.groupID = groupID;
				this.visCheckerType = visCheckerType;
			}

			public Components.Hider.IVisibilityChecker visChecker => _visChecker ??= createVisChecker();
			Components.Hider.IVisibilityChecker _visChecker;

			Components.Hider.IVisibilityChecker createVisChecker()
			{
				var checker = Activator.CreateInstance(visCheckerType) as Components.Hider.IVisibilityChecker;
				Debug.assert(checker != null, $"Options.HideableAttribute: '{visCheckerType}' You need to implement IVisibilityChecker in visCheckerType");

				return checker;
			}
		}
	}
}