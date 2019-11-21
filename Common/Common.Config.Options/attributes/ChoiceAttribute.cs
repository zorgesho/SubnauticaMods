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
			public readonly object[] values = null;

			// using default values, just choice index
			public ChoiceAttribute(params string[] _choices) => choices = _choices;

			// using custom values, parameters should be like ("Choice1", 1.0f, "Choice2", 2.0f etc)
			public ChoiceAttribute(params object[] _choices)
			{																		$"ChoiceAttribute: counts for choices and values is not equal!".logDbgError(_choices != null && _choices.Length % 2 != 0);
				choices = new string[_choices.Length / 2];
				values = new object[_choices.Length / 2];

				for (int i = 0; i < _choices.Length / 2; i++)
				{
					choices[i] = _choices[i * 2] as string;
					values[i] = _choices[i * 2 + 1];
				}
			}
		}
	}
}