using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		// used by FieldAttribute, don't do anything on its own
		[AttributeUsage(AttributeTargets.Field)]
		public class ChoiceAttribute: Config.FieldAttribute
		{
			public readonly string[] choices = null;

			public ChoiceAttribute(params string[] choices_)
			{
				choices = choices_;
			}

			override public void process(object config, FieldInfo field)
			{
				if (choices == null || choices.Length == 0)
					$"Options.ChoiceAttribute.process fieldName:'{field.Name}' Choices not set".logError();
			}
		}
	}
}