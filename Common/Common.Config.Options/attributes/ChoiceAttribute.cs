using System;

namespace Common.Configuration
{
	partial class Options
	{
		// used by FieldAttribute, don't do anything on its own
		[AttributeUsage(AttributeTargets.Field)]
		public class ChoiceAttribute: Attribute
		{
			// we use properties so constructors used only for storing params
			public string[] choices
			{
				get
				{
					processInterleavedParams();
					return _choices;
				}
			}
			public object[] values
			{
				get
				{
					processInterleavedParams();
					return _values;
				}
			}

			string[] _choices = null;
			object[] _values  = null;

			// using default values, just choice index
			public ChoiceAttribute(params string[] choices) => _choices = choices;

			// using custom values, parameters should be like ("Choice1", 1.0f, "Choice2", 2.0f etc)
			public ChoiceAttribute(params object[] interleavedParams) => this.interleavedParams = interleavedParams;

			object[] interleavedParams;

			void processInterleavedParams()
			{
				if (interleavedParams == null)
					return;

				int length = interleavedParams.Length / 2;
				_choices = new string[length];
				_values  = new object[length];

				for (int i = 0; i < length; i++)
				{
					_choices[i] = interleavedParams[i * 2] as string;
					_values[i]  = interleavedParams[i * 2 + 1];
				}

				interleavedParams = null;
			}
		}
	}
}