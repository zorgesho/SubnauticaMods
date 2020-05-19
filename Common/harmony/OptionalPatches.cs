using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Harmony;

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

		// calls setEnabled with result of 'Prepare' method
		public static void update(Type patchType)
		{
			using (Debug.profiler($"Update optional patch: {patchType}", allowNested: false))
			{
				var prepare = patchType.method("Prepare").wrap();
				Debug.assert(prepare);

				if (!prepare)
					return;

				setEnabled(patchType, prepare.invoke<bool>());
			}
		}

		public static void setEnabled(Type patchType, bool val)
		{																														$"OptionalPatches: setEnabled {patchType} => {val}".logDbg();
			if (!(patchType.getAttr<HarmonyPatch>() is HarmonyPatch patch))
				return;

			var method = patch.info.getTargetMethod();

			if (method == null && $"OptionalPatches: method is null!".logError())
				return;

			var prefix = patchType.method("Prefix");
			var postfix = patchType.method("Postfix");
			var transpiler = patchType.method("Transpiler");

			var patches = HarmonyHelper.getPatchInfo(method);

			static bool _contains(IEnumerable<Patch> _list, MethodInfo _method) => _list?.FirstOrDefault(p => p.patch == _method) != null;

			bool prefixActive = _contains(patches?.Prefixes, prefix);
			bool postfixActive = _contains(patches?.Postfixes, postfix);
			bool transpilerActive = _contains(patches?.Transpilers, transpiler);

			if (val)
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