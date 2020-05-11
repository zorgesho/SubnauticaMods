using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options
	{
		public class ChoiceOption: ModOption
		{
			readonly string[] choices;
			readonly object[] values;

			public ChoiceOption(Config.Field cfgField, string label, string[] _choices, object[] _values = null): base(cfgField, label)
			{
				choices = _choices;
				values  = _values;

				// adds choice labels to LanguageHandler, changing array in the process
				for (int i = 0; i < choices.Length; i++)
					registerLabel($"{id}.{i}", ref choices[i]);

				if (id.IndexOf('.') != -1)
					ValidatorPatch.patch();
			}

			public override void addOption(Options options)
			{
				int defaultIndex = values?.findIndex(val => val.Equals(cfgField.value) || val.Equals(cfgField.value.toInt())) ?? cfgField.value.toInt();
				options.AddChoiceOption(id, label, choices, defaultIndex < 0? 0: defaultIndex);
			}

			public override void onValueChange(EventArgs e)
			{
				int? index = (e as ChoiceChangedEventArgs)?.Index;
				cfgField.value = values?[index ?? 0] ?? index;
			}


			// for some reason SMLHelper doesn't allow periods in ChoiceOption's id
			// and we need them for nested classes
			static class ValidatorPatch
			{
				static bool patched = false;

				public static void patch()
				{
					if (patched || !(patched = true))
						return;

					var validateMethod = ReflectionHelper.safeGetType("SMLHelper", "SMLHelper.V2.Options.Utility.Validator")?.method("ValidateID", typeof(string));
					Debug.assert(validateMethod != null);

					HarmonyHelper.patch(validateMethod, typeof(ValidatorPatch).method(nameof(validatorPrefix)));						"SMLHelper validator patched".logDbg();
				}

				static bool validatorPrefix(string id) => id.IndexOf('.') == -1;
			}
		}
	}
}