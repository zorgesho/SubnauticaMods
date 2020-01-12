using System;
using System.Linq;
using System.Reflection;

using Harmony;

namespace Common
{
	static partial class HarmonyHelper
	{
		// dynamic patching/unpatching, for use with OptionalPatch attribute
		public static void setPatchEnabled(bool val, Type type)
		{
			if (Attribute.GetCustomAttribute(type, typeof(OptionalPatchAttribute)) is OptionalPatchAttribute patchAttribute)
				patchAttribute.setEnabled(val, type);
		}


		[AttributeUsage(AttributeTargets.Class)]
		public class OptionalPatchAttribute: Attribute
		{
			readonly MethodInfo method;

			public OptionalPatchAttribute(Type type, string methodName)
			{
				method = type.method(methodName);								$"OptionalPatchAttribute {type} {methodName}".logDbg();
			}

			public void setEnabled(bool val, Type type)
			{
				if (method == null && $"OptionalPatchAttribute method is null!".logError())
					return;

				var prefix = type.method("Prefix");
				var postfix = type.method("Postfix");
				var transpiler = type.method("Transpiler");

				if (val)
				{
					Patches patches = harmonyInstance.GetPatchInfo(method);
					bool patched =  patches != null &&
						(patches.Prefixes.contains(prefix) || patches.Postfixes.contains(postfix) || patches.Transpilers.contains(transpiler));

					if (!patched)
						patch(method, prefix, postfix, transpiler);
					else
						$"OptionalPatchAttribute {method} is already patched".logWarning();
				}
				else
				{
					if (prefix != null)		harmonyInstance.Unpatch(method, prefix);
					if (postfix != null)	harmonyInstance.Unpatch(method, postfix);
					if (transpiler != null) harmonyInstance.Unpatch(method, transpiler);
				}
			}
		}

		static bool contains(this System.Collections.ObjectModel.ReadOnlyCollection<Patch> list, MethodInfo method) =>
			list.FirstOrDefault(p => p.patch == method) != null;
	}
}