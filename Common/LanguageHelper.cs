using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

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
			HarmonyHelper.patch(typeof(LanguageHelper).method(nameof(LanguageHelper._addString)), transpiler: typeof(LanguageHelper).method(nameof(LanguageHelper._patch)));

			// search for any classes that inherited from LanguageHelper and add their static string members to SMLHelper.LanguageHandler
			foreach (var type in ReflectionHelper.definedTypes)
			{
				if (typeof(LanguageHelper).IsAssignableFrom(type))
				{
					foreach (FieldInfo field in type.fields())
					{
						if (field.FieldType == typeof(string) && field.IsStatic && !field.IsLiteral) // const strings are not added to LanguageHandler
						{																						$"LanguageHelper.init: adding field '{field.Name}': '{field.GetValue(null)}'".logDbg();
							if (_addString(prefix + field.Name, field.GetValue(null) as string))
								field.SetValue(null, field.Name); // changing value of string to its name, so we can use it as a string id for 'str' method
						}
					}
				}
			}
		}

		public static string str(string ids) => Language.main == null? ids: (Language.main.TryGet(prefix + ids, out string result)? result: ids);


		// using this to avoid including SMLHelper as a reference to Common project
		// can't use just reflection here (MethodInfo.Invoke), in that case SMLHelper can't identify calling mod (it uses StackTrace for that)
		static bool _addString(string _0, string _1) { "LanguageHelper: SMLHelper is not installed".logWarning(); return false; }
		static IEnumerable<CodeInstruction> _patch(IEnumerable<CodeInstruction> cins)
		{
			try
			{
				if (Assembly.Load("SMLHelper")?.GetType("SMLHelper.V2.Handlers.LanguageHandler")?.GetMethod("SetLanguageLine", BindingFlags.Static | BindingFlags.Public) is MethodInfo SetLanguageLine)
				{
					return new List<CodeInstruction>
					{
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldarg_1),
						new CodeInstruction(OpCodes.Call, SetLanguageLine),
						new CodeInstruction(OpCodes.Ldc_I4_1),
						new CodeInstruction(OpCodes.Ret)
					};
				}
			}
			catch (Exception) {}

			return cins;
		}
	}
}