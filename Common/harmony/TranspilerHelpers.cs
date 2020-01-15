using System;
using System.Diagnostics;
using System.Reflection;

namespace Common
{
	static partial class HarmonyHelper
	{
		static FieldInfo mainConfigField = null; // for using in transpiler helper functions

		static void findConfig(string mainClassName, string configFieldName)
		{
			string modNamespace = new StackTrace().GetFrame(2).GetMethod().ReflectedType.Namespace; // expected to called only from patchAll

			Type mainType = Assembly.GetExecutingAssembly().GetType(modNamespace + "." + mainClassName);

			mainConfigField = mainType?.field(configFieldName);

			if (mainConfigField == null)
				"HarmonyHelper: main config was not found".logWarning();
		}
	}
}