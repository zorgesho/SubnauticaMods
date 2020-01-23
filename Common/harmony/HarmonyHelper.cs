using System;
using System.Reflection;

using Harmony;

namespace Common
{
	static partial class HarmonyHelper
	{
		public static HarmonyInstance harmonyInstance { get; private set; } = null;

		public static void patchAll()
		{
			harmonyInstance = HarmonyInstance.Create(Strings.modName);

			try
			{
				Debug.startStopwatch();
				harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
				Debug.stopStopwatch($"HarmonyHelper.patchAll {Strings.modName}");
			}
			catch (Exception e)
			{
				Debug.stopStopwatch();
				Log.msg(e, "HarmonyHelper.patchAll"); // so the exception will be in the mod's log
				throw e;
			}
		}

		public static void patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
		{																					$"HarmonyHelper.patch: patching '{original}' with prefix:'{prefix}' postfix:'{postfix}' transpiler:'{transpiler}'".logDbg();
			try
			{
				HarmonyMethod _harmonyMethod(MethodInfo method) => (method == null)? null: new HarmonyMethod(method);

				Debug.startStopwatch();
				harmonyInstance.Patch(original, _harmonyMethod(prefix), _harmonyMethod(postfix), _harmonyMethod(transpiler));
				Debug.stopStopwatch("HarmonyHelper.patch");
			}
			catch (Exception e)
			{
				Debug.stopStopwatch();
				Log.msg(e, "HarmonyHelper.patch");
				throw e;
			}
		}
	}
}