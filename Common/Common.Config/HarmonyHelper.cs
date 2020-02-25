using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common.Configuration;

namespace Common
{
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using CIList = List<CodeInstruction>;

	static partial class HarmonyHelper // additional transpilers stuff to work with config
	{
		static readonly FieldInfo mainConfigField = typeof(Config).field("_" + nameof(Config.main)); // for using in transpiler helper functions

		public class UpdateOptionalPatches: Config.Field.ICustomAction { public void customAction() => updateOptionalPatches(); }

		static FieldInfo getCfgVarField(string cfgVarName) =>
			Config.main?.GetType().field(cfgVarName);


		// changing constant to config field
		public static CIList constToCfgVar<T>(CIEnumerable cins, T val, string cfgVarName) =>
			constToCfgVar(cins.ToList(), val, cfgVarName);

		public static CIList constToCfgVar<T>(CIList list, T val, string cfgVarName) =>
			ciReplace(list, ci => ci.isLDC(val), _codeForCfgVar(cfgVarName));


		// changing constant to config field if gameobject have component C
		public static CIList constToCfgVar<T, C>(CIEnumerable cins, T val, string cfgVarName, ILGenerator ilg) where C: Component =>
			constToCfgVar<T, C>(cins.ToList(), val, cfgVarName, ilg);

		public static CIList constToCfgVar<T, C>(CIList list, T val, string cfgVarName, ILGenerator ilg) where C: Component =>
			ciReplace(list, ci => ci.isLDC(val), _codeForCfgVar<T, C>(val, cfgVarName, ilg));


		public static CIEnumerable _codeForCfgVar(string cfgVarName)
		{
			FieldInfo varField = getCfgVarField(cfgVarName);
			Debug.assert(varField != null);

			if (varField == null && $"_codeForCfgVar: varField for {cfgVarName} is not found".logError())
				yield break;

			yield return new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			yield return new CodeInstruction(OpCodes.Ldfld, varField);
		}


		public static CIEnumerable _codeForCfgVar<T, C>(T val, string cfgVarName, ILGenerator ilg) where C: Component
		{																												$"HarmonyHelper._codeForCfgVar: injecting {val} => {cfgVarName} ({typeof(C)})".logDbg();
			FieldInfo varField = getCfgVarField(cfgVarName);
			Debug.assert(varField != null);

			if (varField == null && $"_codeForCfgVar: varField for {cfgVarName} is not found".logError())
				yield break;

			Label lb1 = ilg.DefineLabel();
			Label lb2 = ilg.DefineLabel();

			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Callvirt, typeof(Component).method("GetComponent", new Type[0]).MakeGenericMethod(typeof(C)));
			yield return new CodeInstruction(OpCodes.Ldnull);
			yield return new CodeInstruction(OpCodes.Call, typeof(UnityEngine.Object).method("op_Inequality"));
			yield return new CodeInstruction(OpCodes.Brtrue_S, lb1);

			yield return new CodeInstruction(LdcOpCode.get<T>(), val);
			yield return new CodeInstruction(OpCodes.Br_S, lb2);

			yield return new CodeInstruction(OpCodes.Ldsfld, mainConfigField) { labels = new List<Label>{lb1} };
			yield return new CodeInstruction(OpCodes.Ldfld, varField);
			yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label>{lb2} };
		}
	}
}