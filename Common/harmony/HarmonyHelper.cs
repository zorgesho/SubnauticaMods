using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Harmony;

namespace Common.Harmony
{
	using Reflection;

	// is class have methods that can be used as harmony patches (for more: void patch(Type typeWithPatchMethods))
	public class PatchClassAttribute: Attribute {}

	static class HarmonyHelper
	{
		public static HarmonyInstance harmonyInstance => _harmonyInstance ??= HarmonyInstance.Create(Mod.id);
		static HarmonyInstance _harmonyInstance;

		public static void patchAll(bool searchForPatchClasses = false)
		{
			try
			{
				using (Debug.profiler($"HarmonyHelper.patchAll"))
				{
					harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

					if (searchForPatchClasses)
						ReflectionHelper.definedTypes.Where(type => type.checkAttr<PatchClassAttribute>()).forEach(type => patch(type));
				}
			}
			catch (Exception e)
			{
				Log.msg(e, "HarmonyHelper.patchAll"); // so the exception will be in the mod's log
				throw e;
			}
		}

		public static void patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
		{													$"HarmonyHelper.patch: patching '{original.DeclaringType}.{original.Name}' with prefix:'{prefix}' postfix:'{postfix}' transpiler:'{transpiler}'".logDbg();
			try
			{
				static HarmonyMethod _harmonyMethod(MethodInfo method) => (method == null)? null: new HarmonyMethod(method);

				using (Debug.profiler($"HarmonyHelper.patch '{original.DeclaringType}.{original.Name}'"))
					harmonyInstance.Patch(original, _harmonyMethod(prefix), _harmonyMethod(postfix), _harmonyMethod(transpiler));
			}
			catch (Exception e)
			{
				Log.msg(e, "HarmonyHelper.patch");
				throw e;
			}
		}

		// use methods from 'typeWithPatchMethods' class as harmony patches
		// valid method need to have HarmonyPatch and Harmony[Prefix/Postfix/Transpiler] attributes
		// if typeWithPatchMethods is null, we use type from which this method is called
		public static void patch(Type typeWithPatchMethods = null)
		{
			foreach (var method in (typeWithPatchMethods ?? ReflectionHelper.getCallingType()).methods(BindingFlags.DeclaredOnly))
			{
				MethodInfo _method_if<H>() where H: Attribute => method.checkAttr<H>()? method: null;

				if (method.getAttr<HarmonyPatch>() is HarmonyPatch harmonyPatch)
					patch(harmonyPatch.info.getTargetMethod(), _method_if<HarmonyPrefix>(), _method_if<HarmonyPostfix>(), _method_if<HarmonyTranspiler>());
			}
		}

		public static void unpatch(MethodBase original, MethodInfo patch) => harmonyInstance.Unpatch(original, patch);

		public static Patches getPatchInfo(MethodBase method) => harmonyInstance.GetPatchInfo(method);


		// comparing patches by method's names (for use with shared projects)
		public static bool isPatchedBy(MethodBase original, MethodBase patch)
		{
			Debug.assert(original != null);
			Debug.assert(patch != null);

			var patches = getPatchInfo(original);
			if (patches == null)
				return false;

			string patchFullName = patch.fullName();
			bool _contains(IList<Patch> list) => list.FirstOrDefault(p => p.patch?.fullName() == patchFullName) != null;

			return _contains(patches.Prefixes) || _contains(patches.Postfixes) || _contains(patches.Transpilers);
		}

		public static bool isPatchedBy(MethodBase original, string patchName) =>
			isPatchedBy(original, ReflectionHelper.getCallingType().method(patchName));
	}


	static partial class HarmonyExtensions
	{
		public static MethodBase getTargetMethod(this HarmonyMethod harmonyMethod)
		{
			if (harmonyMethod.methodName != null)
				return harmonyMethod.declaringType?.method(harmonyMethod.methodName, harmonyMethod.argumentTypes);

			if (harmonyMethod.methodType == MethodType.Constructor)
				return harmonyMethod.declaringType?.GetConstructor(ReflectionHelper.bfAll, null, harmonyMethod.argumentTypes, null);

			return null;
		}
	}
}