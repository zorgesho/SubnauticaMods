using System.Reflection;
using Harmony;

namespace Common
{
	static partial class HarmonyHelper
	{
		public static HarmonyInstance harmonyInstance { get; private set; } = null;

		// expected to called only from mod entry function
		public static void patchAll(bool searchConfig = true)
		{
			if (searchConfig)
				findConfig("Main", "config"); // need to be called before harmony patching

			harmonyInstance = HarmonyInstance.Create(Strings.modName);
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}

		public static void patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
		{																					$"HarmonyHelper.patch: patching '{original}' with prefix:'{prefix}' postfix:'{postfix}' transpiler:'{transpiler}'".logDbg();
			try
			{
				harmonyInstance.Patch(original, (prefix == null)? null: new HarmonyMethod(prefix),
												(postfix == null)? null: new HarmonyMethod(postfix),
												(transpiler == null)? null: new HarmonyMethod(transpiler));
			}
			catch (System.Exception e)
			{
				Log.msg(e, "HarmonyHelper.patch");
			}
		}
	}
}