using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

namespace Common
{
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using CIList = List<CodeInstruction>;

	static partial class HarmonyHelper // additional transpilers stuff to work with config
	{
		// changing constant to config field
		public static CIList constToCfgVar<T>(CIEnumerable cins, T val, string cfgVarName) =>
			ciReplace(cins, ci => ci.isLDC(val), _codeForCfgVar(cfgVarName));

		// changing constant to config field if gameobject have component C
		public static CIList constToCfgVar<T, C>(CIEnumerable cins, T val, string cfgVarName, ILGenerator ilg) where C: Component =>
			ciReplace(cins, ci => ci.isLDC(val), _codeForCfgVar<T, C>(val, cfgVarName, ilg));


		public static CIEnumerable _codeForCfgVar(string cfgVarName) // TODO: check labels copy
		{
			FieldInfo varField = mainConfigField?.FieldType.field(cfgVarName);

			if (varField == null && $"_codeForCfgVar: varField for {cfgVarName} is not found".logError())
				yield break;

			yield return new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			yield return new CodeInstruction(OpCodes.Ldfld, varField);
		}


		public static CIEnumerable _codeForCfgVar<T, C>(T val, string cfgVarName, ILGenerator ilg) where C: Component
		{																												$"HarmonyHelper.constToCfgVar: injecting {val} => {cfgVarName} ({typeof(C)})".logDbg();
			FieldInfo varField = mainConfigField?.FieldType.field(cfgVarName);

			if (varField == null && $"_codeForCfgVar: varField for {cfgVarName} is not found".logError())
				yield break;

			Label lb1 = ilg.DefineLabel();
			Label lb2 = ilg.DefineLabel();

			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Callvirt, typeof(Component).method("GetComponent", new Type[0]).MakeGenericMethod(typeof(C)));
			yield return new CodeInstruction(OpCodes.Ldnull);
			yield return new CodeInstruction(OpCodes.Call, typeof(UnityEngine.Object).method("op_Inequality"));
			yield return new CodeInstruction(OpCodes.Brtrue_S, lb1);

			yield return new CodeInstruction(OpCodeByType.get<T>(), val);
			yield return new CodeInstruction(OpCodes.Br_S, lb2);

			yield return new CodeInstruction(OpCodes.Ldsfld, mainConfigField) { labels = new List<Label>{lb1} };
			yield return new CodeInstruction(OpCodes.Ldfld, varField);
			yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label>{lb2} };
		}


		[Obsolete]
		public static CIEnumerable changeConstToConfigVar<T>(CIEnumerable cins, T val, string configVar) => constToCfgVar(cins, val, configVar);
		//{
		//	bool injected = false;

		//	foreach (var ci in cins)
		//	{
		//		if (!injected && ci.isLDC(val))
		//		{																										$"HarmonyHelper.changeConstToConfigVar {configVar}: injected".logDbg();
		//			injected = true;

		//			foreach (var i in _codeForChangeInstructionToConfigVar(configVar, ci))
		//				yield return i;

		//			continue;
		//		}

		//		yield return ci;
		//	}
		//}

		[Obsolete]
		public static CIEnumerable changeConstToConfigVar<T, C>(CIEnumerable cins, T val, string configVar, ILGenerator ilg) where C: Component => constToCfgVar<T, C>(cins, val, configVar, ilg);
		//{
		//	bool injected = false;																						$"HarmonyHelper.changeConstToVar: {val} => {configVar} ({typeof(C)})".logDbg();

		//	foreach (var ci in cins)
		//	{
		//		if (!injected && ci.isLDC(val))
		//		{
		//			injected = true;

		//			foreach (var i in _codeForChangeConstToConfigVar<T, C>(val, configVar, ilg))
		//				yield return i;

		//			continue;
		//		}

		//		yield return ci;
		//	}
		//}

		[Obsolete]
		public static CIEnumerable _codeForChangeInstructionToConfigVar(string configVar, CodeInstruction ci = null) =>
			_codeForCfgVar(configVar);
		//{
		//	FieldInfo varField = mainConfigField?.FieldType.field(configVar);

		//	if (varField == null && $"changeConstToConfigVar: varField for {configVar} is not found".logError())
		//		yield break;

		//	CodeInstruction ldsfld = new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
		//	if (ci != null && ci.labels.Count > 0)
		//		ldsfld.labels.AddRange(ci.labels);

		//	yield return ldsfld;
		//	yield return new CodeInstruction(OpCodes.Ldfld, varField);
		//}

		[Obsolete]
		public static CIEnumerable _codeForChangeConstToConfigVar<T, C>(T value, string configVar, ILGenerator ilg) where C: Component =>
			_codeForCfgVar<T, C>(value, configVar, ilg);
		//{																												$"HarmonyHelper.changeConstToVar: injecting {value} => {configVar} ({typeof(C)})".logDbg();
		//	FieldInfo varField = mainConfigField?.FieldType.field(configVar);

		//	if (varField == null && $"changeConstToConfigVar: varField for {configVar} is not found".logError())
		//		yield break;

		//	Label lb1 = ilg.DefineLabel();
		//	Label lb2 = ilg.DefineLabel();

		//	yield return new CodeInstruction(OpCodes.Ldarg_0);
		//	yield return new CodeInstruction(OpCodes.Callvirt, typeof(Component).method("GetComponent", new Type[0]).MakeGenericMethod(typeof(C)));

		//	yield return new CodeInstruction(OpCodes.Ldnull);
		//	yield return new CodeInstruction(OpCodes.Call, typeof(UnityEngine.Object).method("op_Inequality"));

		//	yield return new CodeInstruction(OpCodes.Brtrue_S, lb1);
		//	yield return new CodeInstruction(OpCodeByType.get<T>(), value);
		//	yield return new CodeInstruction(OpCodes.Br_S, lb2);

		//	var ci1 = new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
		//	ci1.labels.Add(lb1);
		//	yield return ci1;

		//	yield return new CodeInstruction(OpCodes.Ldfld, varField);

		//	var ci2 = new CodeInstruction(OpCodes.Nop);
		//	ci2.labels.Add(lb2);
		//	yield return ci2;
		//}
	}
}