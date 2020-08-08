//#define VALIDATE_PATCHES

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Harmony;

namespace Common.Harmony
{
	using Reflection;

	static partial class HarmonyHelper
	{
		public static HarmonyInstance harmonyInstance => _harmonyInstance ??= HarmonyInstance.Create(Mod.id);
		static HarmonyInstance _harmonyInstance;

		public static void patchAll(bool searchForPatchClasses = false)
		{
			try
			{
#if VALIDATE_PATCHES
				PatchesValidator.validate();
#endif
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