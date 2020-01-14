using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

namespace Common
{
	using Instructions = IEnumerable<CodeInstruction>;
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using CIList = List<CodeInstruction>;
	using CIPredicate = Predicate<CodeInstruction>;

	static partial class HarmonyHelper
	{
		public static class TranspilersHelper
		{
			
		}

		[AttributeUsage(AttributeTargets.Method)]
		public class CheckTranspilerAttribute: Attribute
		{
			public readonly int count;
			public CheckTranspilerAttribute(int _count) => count = _count;
		}


		[Conditional("DEBUG")]
		public static void checkTranspiler(int changes)
		{
			//if (lastTranspilerChanges != changes)
			//	throw new Exception();
		}


		[Conditional("DEBUG")]
		public static void checkTranspilerByStack()
		{
			MethodBase m = new StackTrace().GetFrame(2).GetMethod(); // search

			if (Attribute.GetCustomAttribute(m, typeof(CheckTranspilerAttribute)) is CheckTranspilerAttribute ccc)
			{
				$"------- checkTranspilerByStack {ccc.count}".log();
			}
			
		}

		public static CIList ciList(params object[] cins)
		{
			var list = new CIList();

			foreach (var i in cins)
			{
				switch (i)
				{
					case CIEnumerable ciList:
						list.AddRange(ciList);
						break;
					case OpCode opcode:
						list.Add(new CodeInstruction(opcode));
						break;
					case CodeInstruction ci:
						list.Add(ci);
						break;
					default:
						"!".logError();
						break;
				}
			}

			return list;
		}


		public static CIList ciInsert(CIEnumerable cins, int count, CIPredicate predicate, params object[] toInsert) =>
			ciInsert(cins.ToList(), count, predicate, toInsert);

		public static CIList ciInsert(CIList list, int count, CIPredicate predicate, params object[] toInsert)
		{
			var listToInsert = ciList(toInsert);
			int index, index0 = 0;
			
			do
			{
				if ((index = list.FindIndex(index0, predicate)) == -1)
					break;

				ciInsert(list, index, listToInsert); //list.InsertRange(index, listToInsert);

				index0 += index + listToInsert.Count;
			}
			while (--count != 0);

			return list;
		}

		public static List<CodeInstruction> ciInsert(List<CodeInstruction> list, int index, List<CodeInstruction> listToInsert)
		{
			// check index
			// copy labels ?
			if (index >= 0)
				list.InsertRange(index, listToInsert);

			checkTranspilerByStack();

			return list;
		}


		public static List<CodeInstruction> ciRemove(IEnumerable<CodeInstruction> cins, Predicate<CodeInstruction> predicate, int delta, int count) =>
			ciRemove(cins.ToList(), predicate, delta, count, out _);

		public static List<CodeInstruction> ciRemove(List<CodeInstruction> list, Predicate<CodeInstruction> predicate, int delta, int count) =>
			ciRemove(list, predicate, delta, count, out _);

		public static List<CodeInstruction> ciRemove(List<CodeInstruction> list, Predicate<CodeInstruction> predicate, int delta, int count, out CodeInstruction removed)
		{
			int index = list.FindIndex(predicate);
			return ciRemove(list, (index == -1? -1: index + delta), count, out removed);
		}

		public static List<CodeInstruction> ciRemove(List<CodeInstruction> list, int index, int count, out CodeInstruction removed)
		{
			// check index and count
			removed = list[index];
			list.RemoveRange(index, count);

			return list;
		}


		public static List<CodeInstruction> ciReplace(IEnumerable<CodeInstruction> cins, Predicate<CodeInstruction> predicate, params object[] toReplace) =>
			ciReplace(cins.ToList(), predicate, toReplace);

		public static List<CodeInstruction> ciReplace(List<CodeInstruction> list, Predicate<CodeInstruction> predicate, params object[] toReplace) =>
			ciReplace(list, list.FindIndex(predicate), toReplace);

		public static List<CodeInstruction> ciReplace(List<CodeInstruction> list, int index, params object[] toReplace)
		{
			ciRemove(list, index, 1, out CodeInstruction removed);

			list.InsertRange(index, ciList(toReplace));
			list[index].labels.AddRange(removed.labels);

			return list;
		}




		// changing constant to config field
		public static Instructions constToCfgVar<T>(Instructions cins, T val, string cfgVar)
		{
			return _changeLDCto(cins, val, _codeForCfgVar(cfgVar));
			
			//var list = cins.ToList();

			//int index = list.FindIndex(ci => ci.isLDC(val));
			//list.InsertRange(index, _codeCItoCfgVar(cfgVar, ciRemove(list, index, 1)));

			//return list;
		}

		// changing constant to config field if gameobject have component C
		public static Instructions constToCfgVar<T, C>(Instructions cins, T val, string cfgVar, ILGenerator ilg) where C: Component
		{
			return _changeLDCto(cins, val, _codeForCfgVar<T, C>(val, cfgVar, ilg));
			//var list = cins.ToList();

			//int index = list.FindIndex(ci => ci.isLDC(val));
			//list.InsertRange(index, _codeCItoCfgVar<C>(cfgVar, ciRemove(list, index, 1), ilg));

			//return list;
		}

		public static Instructions _changeLDCto<T>(Instructions cins, T val, IEnumerable<CodeInstruction> cins2)
		{
			var list = cins.ToList();

			ciReplace(list, ci => ci.isLDC(val), cins2);
			//int index = list.FindIndex(ci => ci.isLDC(val));
			//list.InsertRange(index, cins2);

			return list;
		}



		public static Instructions _codeForCfgVar(string configVar)//, CodeInstruction ci = null)
		{
			FieldInfo varField = mainConfigField?.FieldType.field(configVar);

			if (varField == null && $"changeConstToConfigVar: varField for {configVar} is not found".logError())
				yield break;

			CodeInstruction ldsfld = new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			//if (ci != null && ci.labels.Count > 0)
			//	ldsfld.labels.AddRange(ci.labels);

			yield return ldsfld;
			yield return new CodeInstruction(OpCodes.Ldfld, varField);
		}


		public static Instructions _codeForCfgVar<T, C>(T value, string configVar, /*CodeInstruction ci,*/ ILGenerator ilg) where C: Component
		{																												$"HarmonyHelper.changeConstToVar: injecting {value} => {configVar} ({typeof(C)})".logDbg();
			FieldInfo varField = mainConfigField?.FieldType.field(configVar);

			if (varField == null && $"changeConstToConfigVar: varField for {configVar} is not found".logError())
				yield break;

			Label lb1 = ilg.DefineLabel();
			Label lb2 = ilg.DefineLabel();

			yield return new CodeInstruction(OpCodes.Ldarg_0);// { labels = ci.labels};
			yield return new CodeInstruction(OpCodes.Callvirt, typeof(Component).method("GetComponent", new Type[0]).MakeGenericMethod(typeof(C)));

			yield return new CodeInstruction(OpCodes.Ldnull);
			yield return new CodeInstruction(OpCodes.Call, typeof(UnityEngine.Object).method("op_Inequality"));

			yield return new CodeInstruction(OpCodes.Brtrue_S, lb1);
			yield return new CodeInstruction(OpCodeByType.get<T>(), value);
			//yield return new CodeInstruction(ci.opcode, ci.operand);
			yield return new CodeInstruction(OpCodes.Br_S, lb2);

			var ci1 = new CodeInstruction(OpCodes.Ldsfld, mainConfigField);// { labels = new List<Label>{lb1} };
			ci1.labels.Add(lb1);
			yield return ci1;

			yield return new CodeInstruction(OpCodes.Ldfld, varField);

			var ci2 = new CodeInstruction(OpCodes.Nop);
			ci2.labels.Add(lb2);
			yield return ci2;
		}
	}
}