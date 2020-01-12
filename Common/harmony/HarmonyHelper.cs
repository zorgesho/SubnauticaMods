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

			Debug.startStopwatch();
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
			Debug.stopStopwatch($"Harmony.PatchAll {Strings.modName}");
		}

		public static void patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
		{																					$"HarmonyHelper.patch: patching '{original}' with prefix:'{prefix}' postfix:'{postfix}' transpiler:'{transpiler}'".logDbg();
			try
			{
				Debug.startStopwatch();
				harmonyInstance.Patch(original, (prefix == null)? null: new HarmonyMethod(prefix),
												(postfix == null)? null: new HarmonyMethod(postfix),
												(transpiler == null)? null: new HarmonyMethod(transpiler));
				Debug.stopStopwatch($"HarmonyHelper.patch");
			}
			catch (System.Exception e)
			{
				Log.msg(e, "HarmonyHelper.patch");
			}
		}
	}
}