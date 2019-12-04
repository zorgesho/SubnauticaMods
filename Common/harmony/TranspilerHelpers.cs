using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using Harmony;

namespace Common
{
	using Instructions = IEnumerable<CodeInstruction>;

	static partial class HarmonyHelper
	{
		static FieldInfo mainConfigField = null; // for using in transpiler helper functions
		
		static void findConfig(string mainClassName, string configFieldName)
		{
			string modNamespace = new StackTrace().GetFrame(2).GetMethod().ReflectedType.Namespace; // expected to called only from patchAll

			Type mainType = Assembly.GetExecutingAssembly().GetType(modNamespace + "." + mainClassName);

			mainConfigField = mainType?.GetField(configFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			if (mainConfigField == null)
				"HarmonyHelper: main config was not found".logWarning();
		}

		// changing constant to config field
		public static Instructions changeConstToConfigVar<T>(Instructions cins, T val, string configVar)
		{
			bool injected = false;

			foreach (var ci in cins)
			{
				if (!injected && ci.isLDC(val))
				{																												"HarmonyHelper.changeConstToConfigVar: injected".logDbg();
					injected = true;

					foreach (var i in _codeForChangeInstructionToConfigVar(configVar, ci))
						yield return i;

					continue;
				}

				yield return ci;
			}
		}

		// changing constant to config field if gameobject have component C
		public static Instructions changeConstToConfigVar<T, C>(Instructions cins, T val, string configVar, ILGenerator ilg) where C: Component
		{
			bool injected = false;																						"HarmonyHelper.changeConstToVar".logDbg();

			foreach (var ci in cins)
			{																											
				if (!injected && ci.isLDC(val))
				{																										"HarmonyHelper.changeConstToVar: injected".logDbg();
					injected = true;

					foreach (var i in _codeForChangeConstToConfigVar<T, C>(val, configVar, ilg))
						yield return i;

					continue;
				}

				yield return ci;
			}
		}


		public static Instructions _codeForChangeInstructionToConfigVar(string configVar, CodeInstruction ci = null)
		{
			FieldInfo varField = mainConfigField?.FieldType.GetField(configVar, _BindingFlags.all);

			if (varField == null)
			{
				$"changeConstToConfigVar: varField for {configVar} is not found".logError();
				yield return ci;
			}

			CodeInstruction ldsfld = new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			if (ci != null && ci.labels.Count > 0)
				ldsfld.labels.AddRange(ci.labels);

			yield return ldsfld;
			yield return new CodeInstruction(OpCodes.Ldfld, varField);
		}


		public static Instructions _codeForChangeConstToConfigVar<T, C>(T value, string configVar, ILGenerator ilg) where C: Component
		{
			FieldInfo varField = mainConfigField?.FieldType.GetField(configVar, _BindingFlags.all);

			if (varField == null)
			{
				$"changeConstToConfigVar: varField for {configVar} is not found".logError();
				yield break;
			}

			Label lb1 = ilg.DefineLabel();
			Label lb2 = ilg.DefineLabel();

			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.Method(typeof(Component), "GetComponent").MakeGenericMethod(typeof(C)));

			yield return new CodeInstruction(OpCodes.Ldnull);
			yield return new CodeInstruction(OpCodes.Call,
				AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality"));

			yield return new CodeInstruction(OpCodes.Brtrue_S, lb1);
			yield return new CodeInstruction(OpCodeByType.get<T>(), value);
			yield return new CodeInstruction(OpCodes.Br_S, lb2);

			var ci1 = new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			ci1.labels.Add(lb1);
			yield return ci1;

			yield return new CodeInstruction(OpCodes.Ldfld, varField);

			var ci2 = new CodeInstruction(OpCodes.Nop);
			ci2.labels.Add(lb2);
			yield return ci2;
		}

		//public static Instructions _codeForChangeConstToConfigMethodCall(string configMethod)
		//{
		//	MethodInfo method = mainConfigField?.FieldType.GetMethod(configMethod, _BindingFlags.all);
			
		//	if (method == null)
		//	{
		//		$"changeConstToConfigMethodCall: method '{configMethod}' is not found".logError();
		//		yield break;
		//	}

		//	yield return new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
		//	yield return new CodeInstruction(OpCodes.Callvirt, method);
		//}
	}
}