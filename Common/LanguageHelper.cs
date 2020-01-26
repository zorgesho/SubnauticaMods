using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Common
{
	// for allowing to use SMLHelper language override files
	class LanguageHelper
	{
		static bool inited = false;

		// internally using prefix for id strings to avoid conflicts with different mods (all strings are end up in one common list)
		// don't use prefix in mod's code
		static readonly string prefix = Strings.modName + ".";

		public static void init()
		{
			if (inited)
				return;

			inited = true;

			// search for any classes that inherited from LanguageHelper and add their static string members to SMLHelper.LanguageHandler
			foreach (var type in ReflectionHelper.definedTypes.Where(type => typeof(LanguageHelper).IsAssignableFrom(type)))
			{
				foreach (FieldInfo field in type.fields())
				{
					if (field.FieldType == typeof(string) && field.IsStatic && !field.IsLiteral) // const strings are not added to LanguageHandler
					{																						$"LanguageHelper.init: adding field '{field.Name}': '{field.GetValue(null)}'".logDbg();
						if (addString(prefix + field.Name, field.GetValue(null) as string))
							field.SetValue(null, field.Name); // changing value of string to its name, so we can use it as a string id for 'str' method
					}
				}
			}
		}

		// get string by id from Language.main
		public static string str(string ids) => Language.main == null? ids: (Language.main.TryGet(prefix + ids, out string result)? result: ids);

		// wrap method for SMLHelper.LanguageHandler.SetLanguageLine
		static readonly Func<string, string, bool> addString = _initDynamicMethod();

		// using this to avoid including SMLHelper as a reference to Common project
		// can't use just reflection here (MethodInfo.Invoke), in that case SMLHelper can't identify calling mod (it uses StackTrace for that)
		static Func<string, string, bool> _initDynamicMethod()
		{
			MethodInfo SetLanguageLine = ReflectionHelper.safeGetMethod("SMLHelper", "SMLHelper.V2.Handlers.LanguageHandler", "SetLanguageLine");
			Debug.assert(SetLanguageLine != null);

			DynamicMethod dm = new DynamicMethod("_addString", typeof(bool), new Type[] { typeof(string), typeof(string)}, typeof(LanguageHelper));

			ILGenerator ilg = dm.GetILGenerator();
			if (SetLanguageLine != null)
			{
				ilg.Emit(OpCodes.Ldarg_0);
				ilg.Emit(OpCodes.Ldarg_1);
				ilg.Emit(OpCodes.Call, SetLanguageLine);
				ilg.Emit(OpCodes.Ldc_I4_1);
			}
			else
			{
				ilg.Emit(OpCodes.Ldc_I4_0);
			}
			ilg.Emit(OpCodes.Ret);

			return dm.createDelegate<Func<string, string, bool>>();
		}
	}
}