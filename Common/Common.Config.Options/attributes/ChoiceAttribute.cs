using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		// used by FieldAttribute, don't do anything on its own
		[AttributeUsage(AttributeTargets.Field)]
		public class ChoiceAttribute: Attribute
		{
			public readonly string[] choices = null;

			public ChoiceAttribute(params string[] _choices)
			{
				choices = _choices;

				if (choices == null || choices.Length == 0)
					$"Options.ChoiceAttribute.process:  Choices not set".logError();
			}
		}
	}
}