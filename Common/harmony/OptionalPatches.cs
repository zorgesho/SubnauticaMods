using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Harmony;

namespace Common
{
	static partial class HarmonyHelper
	{
		[AttributeUsage(AttributeTargets.Class)]
		public class OptionalPatchAttribute: Attribute {}

		// dynamic patching/unpatching
		public static void updateOptionalPatches()				=> OptionalPatches.updatePatches();
		public static void updateOptionalPatch(Type type)		=> OptionalPatches.updatePatch(type);
		public static void setPatchEnabled(bool val, Type type) => OptionalPatches.setEnabled(val, type);

		static class OptionalPatches
		{
			static List<Type> optionalPatches = null;

			public static void updatePatches()
			{
				optionalPatches ??= ReflectionHelper.definedTypes.Where(type => type.checkAttr<OptionalPatchAttribute>()).ToList();

				using (Debug.profiler("Update optional patches"))
					optionalPatches.ForEach(type => updatePatch(type));
			}

			// calls setEnabled with result of 'Prepare' method
			public static void updatePatch(Type patchType)
			{
				using (Debug.profiler($"Update optional patch: {patchType}", allowNested: false))
				{
					var prepare = patchType.methodWrap("Prepare");
					Debug.assert(prepare);

					if (!prepare)
						return;

					setEnabled(prepare.invoke<bool>(), patchType);
				}
			}


			public static void setEnabled(bool val, Type patchType)
			{																														$"OptionalPatches: setEnabled {patchType} => {val}".logDbg();
				if (!(patchType.getAttr<HarmonyPatch>() is HarmonyPatch patch))
					return;

				var method = patch.info.getTargetMethod();

				if (method == null && $"OptionalPatches: method is null!".logError())
					return;

				var prefix = patchType.method("Prefix");
				var postfix = patchType.method("Postfix");
				var transpiler = patchType.method("Transpiler");

				var patches = getPatchInfo(method);

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
					if (prefixActive)	  harmonyInstance.Unpatch(method, prefix);
					if (postfixActive)	  harmonyInstance.Unpatch(method, postfix);
					if (transpilerActive) harmonyInstance.Unpatch(method, transpiler);
				}
			}
		}
	}
}