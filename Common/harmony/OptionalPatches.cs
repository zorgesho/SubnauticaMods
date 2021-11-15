using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using HarmonyLib;

namespace Common.Harmony
{
	using Reflection;

	[AttributeUsage(AttributeTargets.Class)]
	public class OptionalPatchAttribute: Attribute {}

	static class OptionalPatches // dynamic patching/unpatching
	{
		static List<Type> optionalPatches = null;

		public static void update()
		{
			optionalPatches ??= ReflectionHelper.definedTypes.Where(type => type.checkAttr<OptionalPatchAttribute>()).ToList();

			using (Debug.profiler("Update optional patches"))
				optionalPatches.ForEach(type => update(type));
		}

		// calls setEnabled with result of 'prepare' method
		public static void update(Type patchType)
		{
			using var _ = Debug.profiler($"Update optional patch: {patchType}", allowNested: false);

			var prepare = patchType.method("prepare", ReflectionHelper.bfAll | BindingFlags.IgnoreCase).wrap();
			Debug.assert(prepare);

			if (prepare)
				setEnabled(patchType, prepare.invoke<bool>());
		}

		public static void setEnabled(Type patchType, bool enabled)
		{
			if (patchType.getAttr<HarmonyPatch>() is HarmonyPatch patch) // regular harmony patch
				setEnabled(patchType, patch, enabled);
			else if (patchType.checkAttr<PatchClassAttribute>()) // optional patch class
				HarmonyHelper.patch(patchType, enabled);
		}

		static void setEnabled(Type patchType, HarmonyPatch patch, bool enabled)
		{																									$"OptionalPatches: setEnabled {patchType} => {enabled}".logDbg();
			var method = patch.info.getTargetMethod();

			if (method == null)
			{
				"OptionalPatches: method is null!".logError();
				return;
			}

			var prefix = patchType.method("Prefix");
			var postfix = patchType.method("Postfix");
			var transpiler = patchType.method("Transpiler");

			var patches = HarmonyHelper.getPatchInfo(method);

			bool prefixActive = patches.isPatchedBy(prefix);
			bool postfixActive = patches.isPatchedBy(postfix);
			bool transpilerActive = patches.isPatchedBy(transpiler);

			if (enabled)
			{
				if (!prefixActive && !postfixActive && !transpilerActive)
					HarmonyHelper.patch(method, prefix, postfix, transpiler);
			}
			else
			{
				// need to check if this is actual patches to avoid unnecessary updates in harmony (with transpilers especially)
				if (prefixActive)	  HarmonyHelper.unpatch(method, prefix);
				if (postfixActive)	  HarmonyHelper.unpatch(method, postfix);
				if (transpilerActive) HarmonyHelper.unpatch(method, transpiler);
			}
		}
	}
}