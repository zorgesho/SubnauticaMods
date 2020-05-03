using System;
using System.Linq;
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
		// don't use prefix in mod's code unless you're sure you need it
		static readonly string prefix = Mod.id + ".";

		public static void init()
		{
			if (inited || !(inited = true))
				return;

			// search for any classes that inherited from LanguageHelper and add their public static string members to SMLHelper.LanguageHandler
			ReflectionHelper.definedTypes.
				Where(type => typeof(LanguageHelper).IsAssignableFrom(type)).
				SelectMany(type => type.fields()).
				Where(field => field.FieldType == typeof(string) && field.IsStatic && field.IsPublic && !field.IsLiteral). // const strings are not added to LanguageHandler
				forEach(field => field.SetValue(null, add(field.Name, field.GetValue(null) as string))); // changing value of string to its name, so we can use it as a string id for 'str' method
		}

		// get string by id from Language.main
		public static string str(string ids) =>
			(ids == null || Language.main == null)? ids: (Language.main.TryGet(prefix + ids, out string result)? result: ids);

		// add string to LanguageHandler, use getFullID if you need to get ids with prefix (e.g. for UI labels)
		public static string add(string ids, string str, bool getFullID = false)
		{																							$"LanguageHelper: adding string '{ids}': '{str}'".logDbg();
			string fullID = prefix + ids;
			return addString(fullID, str)? (getFullID? fullID: ids): str;
		}


		static Dictionary<string, string> substitutedStrings = null; // 'key' string using value of 'value' string

		// use 'substituteStringID' string as value for 'stringID' (for using before Language.main is loaded)
		public static void substituteString(string stringID, string substituteStringID)
		{
			if (substitutedStrings == null)
			{
				substitutedStrings = new Dictionary<string, string>();

				HarmonyHelper.patch(typeof(Language).method("LoadLanguageFile"),
					postfix: typeof(LanguageHelper).method(nameof(substituteStrings)));
			}

			substitutedStrings[stringID] = substituteStringID;
		}

		[HarmonyPriority(Priority.Low)]
		static void substituteStrings(Language __instance) =>
			substitutedStrings.forEach(subst => __instance.strings[subst.Key] = __instance.strings[subst.Value]);


		// wrap method for SMLHelper.LanguageHandler.SetLanguageLine
		static readonly Func<string, string, bool> addString = _initDynamicMethod();

		// using this to avoid including SMLHelper as a reference to Common project
		// can't use just reflection here (MethodInfo.Invoke), in that case SMLHelper can't identify calling mod (it uses StackTrace for that)
		static Func<string, string, bool> _initDynamicMethod()
		{
			MethodInfo SetLanguageLine = ReflectionHelper.safeGetType("SMLHelper", "SMLHelper.V2.Handlers.LanguageHandler")?.method("SetLanguageLine");
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