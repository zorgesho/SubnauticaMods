using System;
using System.Linq;
using System.Reflection;

using Harmony;

namespace Common
{
	static partial class HarmonyHelper
	{
		public static HarmonyInstance harmonyInstance { get; private set; } = null;

		// is class have methods that can be used as harmony patches (for more: void patch(Type typeWithPatchMethods))
		public class PatchClassAttribute: Attribute {}

		public static void patchAll(bool searchForPatchClasses = false)
		{
			harmonyInstance = HarmonyInstance.Create(Strings.modName);

			try
			{
				Debug.startStopwatch();
				harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
				if (searchForPatchClasses)
					ReflectionHelper.definedTypes.Where(type => Attribute.GetCustomAttribute(type, typeof(PatchClassAttribute)) != null).forEach(type => patch(type));
				Debug.stopStopwatch($"HarmonyHelper.patchAll {Strings.modName}");
			}
			catch (Exception e)
			{
				Debug.stopStopwatch();
				Log.msg(e, "HarmonyHelper.patchAll"); // so the exception will be in the mod's log
				throw e;
			}
		}

		public static void patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
		{																					$"HarmonyHelper.patch: patching '{original}' with prefix:'{prefix}' postfix:'{postfix}' transpiler:'{transpiler}'".logDbg();
			try
			{
				HarmonyMethod _harmonyMethod(MethodInfo method) => (method == null)? null: new HarmonyMethod(method);

				Debug.startStopwatch();
				harmonyInstance.Patch(original, _harmonyMethod(prefix), _harmonyMethod(postfix), _harmonyMethod(transpiler));
				Debug.stopStopwatch("HarmonyHelper.patch");
			}
			catch (Exception e)
			{
				Debug.stopStopwatch();
				Log.msg(e, "HarmonyHelper.patch");
				throw e;
			}
		}

		// use methods from 'typeWithPatchMethods' class as harmony patches
		// valid method need to have HarmonyPatch and Harmony[Prefix/Postfix/Transpiler] attributes
		public static void patch(Type typeWithPatchMethods = null)
		{
			foreach (var method in (typeWithPatchMethods ?? ReflectionHelper.getCallingType()).methods())
			{
				if (Attribute.GetCustomAttribute(method, typeof(HarmonyPatch)) is HarmonyPatch harmonyPatch)
				{
					MethodInfo _method_if<PatchType>() => Attribute.GetCustomAttribute(method, typeof(PatchType)) != null? method: null;

					patch(harmonyPatch.info.getTargetMethod(), _method_if<HarmonyPrefix>(), _method_if<HarmonyPostfix>(), _method_if<HarmonyTranspiler>());
				}
			}
		}

		static MethodInfo getTargetMethod(this HarmonyMethod harmonyMethod) => harmonyMethod.declaringType?.method(harmonyMethod.methodName);
	}
}