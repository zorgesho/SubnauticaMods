using System;
using System.Linq;
using System.Collections.Generic;

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
			string[] _choices;

			public object[] values
			{
				get
				{
					processInterleavedParams();
					return _values;
				}
			}
			object[] _values;

			// using default values, just choice index
			public ChoiceAttribute(params string[] choices) => _choices = choices;

			// using custom values, parameters should be like ("Choice1", 1.0f, "Choice2", 2.0f etc)
			public ChoiceAttribute(params object[] interleavedParams) => this.interleavedParams = interleavedParams;

			object[] interleavedParams;

			void processInterleavedParams()
			{
				if (interleavedParams != null)
					InterleavedParams.split(interleavedParams, out _choices, out _values);

				interleavedParams = null;
			}
		}


		[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
		public class ChoiceMasterAttribute: Attribute
		{
			public readonly object choiceValue;
			readonly object[] interleavedParams;

			public List<Tuple<string, object>> dependants
			{
				get
				{
					if (_dependants == null)
					{
						InterleavedParams.split(interleavedParams, out string[] fields, out object[] values);
						_dependants = Enumerable.Range(0, fields.Length).Select(i => Tuple.Create(fields[i], values[i])).ToList();
					}

					return _dependants;
				}
			}
			List<Tuple<string, object>> _dependants;

			public ChoiceMasterAttribute(object choiceValue, params object[] interleavedParams)
			{
				this.choiceValue = choiceValue;
				this.interleavedParams = interleavedParams;
			}
		}


		static class InterleavedParams
		{
			public static void split(object[] iparams, out string[] strings, out object[] values)
			{
				Debug.assert(validate(iparams));

				int length = iparams.Length / 2;
				strings = Enumerable.Range(0, length).Select(i => iparams[i * 2] as string).ToArray();
				values  = Enumerable.Range(0, length).Select(i => iparams[i * 2 + 1]).ToArray();
			}

			static bool validate(object[] iparams)
			{
				if (iparams.Length % 2 != 0)
					return false;

				for (int i = 0; i < iparams.Length; i++)
				{
					if (iparams[i] == null)
						return false;

					var type = iparams[i].GetType();

					if (i % 2 == 0 && type != typeof(string))
						return false;

					if (i % 2 == 1 && !type.IsEnum && type != typeof(int) && type != typeof(float))
						return false;
				}

				return true;
			}
		}
	}
}