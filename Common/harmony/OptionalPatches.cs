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
		public static void setPatchEnabled(bool val, Type type) => OptionalPatches.setEnabled(val, type);

		static class OptionalPatches
		{
			static List<Type> optionalPatches = null;

			public static void updatePatches()
			{
				if (optionalPatches == null)
					optionalPatches = ReflectionHelper.definedTypes.Where(type => Attribute.GetCustomAttribute(type, typeof(OptionalPatchAttribute)) != null).ToList();

				optionalPatches.ForEach(type => updatePatch(type));
			}

			// calls setEnabled with result of 'Prepare' method
			static void updatePatch(Type patchType)
			{
				MethodInfo prepare = patchType.method("Prepare");
				Debug.assert(prepare != null);

				if (prepare == null)
					return;

				bool? res = prepare.Invoke(null, null) as bool?;

				if (res == null)
					return;

				setEnabled((bool)res, patchType);
			}


			public static void setEnabled(bool val, Type patchType)
			{																														$"OptionalPatches: setEnabled {patchType} => {val}".logDbg();
				if (!(Attribute.GetCustomAttribute(patchType, typeof(HarmonyPatch)) is HarmonyPatch patch))
					return;

				MethodInfo method = patch.info.declaringType?.method(patch.info.methodName);

				if (method == null && $"OptionalPatches: method is null!".logError())
					return;

				var prefix = patchType.method("Prefix");
				var postfix = patchType.method("Postfix");
				var transpiler = patchType.method("Transpiler");

				if (val)
				{
					bool _contains(IEnumerable<Patch> _list, MethodInfo _method) => _list.FirstOrDefault(p => p.patch == _method) != null;

					Patches patches = harmonyInstance.GetPatchInfo(method);
					bool patched =  patches != null &&
						(_contains(patches.Prefixes, prefix) || _contains(patches.Postfixes, postfix) || _contains(patches.Transpilers, transpiler));

					if (!patched)
						HarmonyHelper.patch(method, prefix, postfix, transpiler);
				}
				else
				{
					if (prefix != null)		harmonyInstance.Unpatch(method, prefix);
					if (postfix != null)	harmonyInstance.Unpatch(method, postfix);
					if (transpiler != null) harmonyInstance.Unpatch(method, transpiler);
				}
			}
		}
	}
}