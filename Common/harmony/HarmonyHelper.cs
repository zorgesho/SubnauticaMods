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
		// patchStatus - null:  patch without checking patched status (with call of 'prepare' method), true: patch, if not already patched, false: unpatch, if already patched
		public static void patch(Type typeWithPatchMethods = null, bool? patchStatus = null)
		{
			typeWithPatchMethods ??= ReflectionHelper.getCallingType();

			if (patchStatus == null && typeWithPatchMethods.method("prepare")?.wrap().invoke<bool>() == false)
				return; // if val != null, 'prepare' probably called in OptionalPatches already, but we also can call 'patch' method directly

			bool? patched = null; // we will check only first method with patch attribute, they all should have same patch status anyway

			foreach (var method in typeWithPatchMethods.methods(ReflectionHelper.bfAll | BindingFlags.DeclaredOnly))
			{
				if (!(method.getAttr<HarmonyPatch>() is HarmonyPatch harmonyPatch))
					continue;

				var targetMethod = harmonyPatch.info.getTargetMethod();

				bool _isPatched() => patched ??= isPatchedBy(targetMethod, method);

				if (patchStatus == null || (patchStatus == true && !_isPatched()))
				{
					MethodInfo _method_if<H>() where H: Attribute => method.checkAttr<H>()? method: null;
					patch(targetMethod, _method_if<HarmonyPrefix>(), _method_if<HarmonyPostfix>(), _method_if<HarmonyTranspiler>());
				}
				else if (patchStatus == false && _isPatched())
				{
					unpatch(targetMethod, method);
				}
			}
		}

		public static void unpatch(MethodBase original, MethodInfo patch) => harmonyInstance.Unpatch(original, patch);

		public static Patches getPatchInfo(MethodBase method) => harmonyInstance.GetPatchInfo(method);

		// checkByName - comparing patches by method's names (for use with shared projects)
		public static bool isPatchedBy(MethodBase original, MethodBase patch, bool checkByName = false)
		{
			Debug.assert(original != null && patch != null, $"'{original}' '{patch}'");
			return getPatchInfo(original).isPatchedBy(patch, checkByName);
		}
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

		public static bool isPatchedBy(this Patches patches, MethodBase patch, bool checkByName = false)
		{
			if (patches == null)
				return false;

			string patchFullName = checkByName? patch.fullName(): null;

			bool _contains(IList<Patch> list) => list.Count > 0 &&
				list.Any(p => (checkByName && p.patch?.fullName() == patchFullName) || (!checkByName && Equals(p.patch, patch)));

			return _contains(patches.Prefixes) || _contains(patches.Postfixes) || _contains(patches.Transpilers);
		}
	}
}