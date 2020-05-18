using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

namespace Common.Harmony
{
	using Reflection;
	using Configuration;

	using CIEnumerable = IEnumerable<CodeInstruction>;
	using CIList = List<CodeInstruction>;

	class UpdateOptionalPatches: Config.Field.IAction { public void action() => OptionalPatches.update(); }

	static partial class CIHelper // additional transpiler stuff to work with config
	{
		// for using in transpiler helper functions
		static readonly MethodInfo mainConfig = typeof(Config).property(nameof(Config.main)).GetGetMethod();

		// warning: nested classes are not supported for cfgVarName!

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


		static CodeInstruction getCfgVarCI(string cfgVarName)
		{
			if (Config.main == null)
				return null;

			if (Config.main.GetType().field(cfgVarName) is FieldInfo varField)
				return new CodeInstruction(OpCodes.Ldfld, varField);

			if (Config.main.GetType().property(cfgVarName)?.GetGetMethod() is MethodInfo varGetter)
				return new CodeInstruction(OpCodes.Callvirt, varGetter);

			return null;
		}

		public static CIEnumerable _codeForCfgVar(string cfgVarName)
		{
			var cfgVarCI = getCfgVarCI(cfgVarName);
			Debug.assert(cfgVarCI != null, $"_codeForCfgVar: member for {cfgVarName} is not found");

			if (cfgVarCI == null)
				yield break;

			yield return new CodeInstruction(OpCodes.Call, mainConfig);
			yield return cfgVarCI;
		}


		public static CIEnumerable _codeForCfgVar<T, C>(T val, string cfgVarName, ILGenerator ilg) where C: Component
		{																												$"HarmonyHelper._codeForCfgVar: injecting {val} => {cfgVarName} ({typeof(C)})".logDbg();
			var cfgVarCI = getCfgVarCI(cfgVarName);
			Debug.assert(cfgVarCI != null, $"_codeForCfgVar: member for {cfgVarName} is not found");

			if (cfgVarCI == null)
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

			yield return new CodeInstruction(OpCodes.Call, mainConfig) { labels = new List<Label>{lb1} };
			yield return cfgVarCI;
			yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label>{lb2} };
		}
	}
}