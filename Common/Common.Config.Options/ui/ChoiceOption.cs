using System;
using System.Collections.Generic;

using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options
	{
		partial class Factory
		{
			class ChoiceOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type.IsEnum && cfgField.type != typeof(UnityEngine.KeyCode)) // add choice option for enum
					{
						var values = new List<object>();
						foreach (var val in Enum.GetValues(cfgField.type))
							values.Add(val.toInt());

						return new ChoiceOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label, Enum.GetNames(cfgField.type), values.ToArray());
					}

					if (cfgField.type == typeof(float) || cfgField.type == typeof(int)) // creating ChoiceOption if we also have choice attribute
					{
						if (cfgField.getAttr<ChoiceAttribute>() is ChoiceAttribute choice && choice.choices.Length > 0)
							return new ChoiceOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label, choice.choices, choice.values);
					}

					return null;
				}
			}
		}


		public class ChoiceOption: ModOption
		{
			readonly string[] choices;
			readonly object[] values;

			public ChoiceOption(Config.Field cfgField, string label, string[] choices, object[] values = null): base(cfgField, label)
			{
				this.choices = choices;
				this.values  = values;

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