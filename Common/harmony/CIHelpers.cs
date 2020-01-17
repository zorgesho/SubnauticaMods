using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

namespace Common
{
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using CIList = List<CodeInstruction>;
	using CIPredicate = Predicate<CodeInstruction>;

	static partial class HarmonyHelper
	{
#region CodeInstruction extensions

		public static bool isLDC<T>(this CodeInstruction ci, T val) => ci.isOp(LdcOpCode.get<T>(), val);

		public static bool isOp(this CodeInstruction ci, OpCode opcode, object operand = null) =>
			ci.opcode == opcode && (operand == null || ci.operand.Equals(operand));

		public static void log(this CodeInstruction ci) => $"{ci.opcode} {ci.operand}".log();

		public static void log(this CIEnumerable cins, bool searchFirstOps = false)
		{
			var list = cins.ToList();

			int _findLabel(object label) => // find target index for jumps
				list.FindIndex(_ci => _ci.labels?.FindIndex(l => l.Equals(label)) != -1);

			string _labelsInfo(CodeInstruction ci)
			{
				if ((ci.labels?.Count ?? 0) == 0)
					return "";

				string res = $"=> labels({ci.labels.Count}): ";
				foreach (var l in ci.labels)
					res += "Label" + l.GetHashCode() + " ";

				return res;
			}

			for (int i = 0; i < list.Count; i++)
			{
				var ci = list[i];

				int labelIndex = (ci.operand?.GetType() == typeof(Label))? _findLabel(ci.operand): -1;
				string operandInfo = labelIndex != -1? "jump to " + labelIndex: ci.operand?.ToString();
				string isFirstOp = (searchFirstOps && list.FindIndex(_ci => _ci.opcode == ci.opcode) == i)? " 1ST":""; // is such an opcode is first encountered in this instruction

				$"{i}{isFirstOp}: {ci.opcode} {operandInfo} {_labelsInfo(ci)}".log();
			}
		}
#endregion

#region CodeInstruction sequences manipulation methods

		#region CIList methods
		// makes list with CodeInstructions from various objects (see 'switch' for object types)
		public static CIList toCIList(params object[] cins)
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
						Debug.assert(false, $"toCIList: unsupported type {i.GetType()}");
						break;
				}
			}

			return list;
		}

		// makes new list with cloned CodeInstructions
		public static CIList copyCIList(CIList list)
		{
			var newList = new CIList();

			list.ForEach(ci => newList.Add(ci.Clone()));

			return newList;
		}
		#endregion

		#region ciInsert
		// maxMatchCount = 0 for all predicate matches
		// indexDelta - change actual index from matched for insertion
		// if indexDelta is 0 than cinsToInsert will be inserted right before finded instruction
		// throws assert exception if there were no matches at all or if maxMatchCount > 0 and there were less predicate matches
		public static CIList ciInsert(CIEnumerable cins, CIPredicate predicate, int indexDelta, int maxMatchCount, params object[] cinsToInsert) =>
			ciInsert(cins.ToList(), predicate, indexDelta, maxMatchCount, cinsToInsert);

		// for just first predicate match (insert right after finded instruction)
		public static CIList ciInsert(CIEnumerable cins, CIPredicate predicate, params object[] cinsToInsert) =>
			ciInsert(cins.ToList(), predicate, cinsToInsert);

		// for just first predicate match (insert right after finded instruction)
		public static CIList ciInsert(CIList list, CIPredicate predicate, params object[] cinsToInsert) =>
			ciInsert(list, predicate, 1, 1, cinsToInsert);

		public static CIList ciInsert(CIList list, CIPredicate predicate, int indexDelta, int maxMatchCount, params object[] cinsToInsert)
		{
			bool anyInserts = false; // just for assert
			int index, index0 = 0;

			var listToInsert = toCIList(cinsToInsert);
			int indexIncrement = listToInsert.Count + Math.Max(1, indexDelta);

			while ((index = list.FindIndex(index0, predicate)) != -1 && (anyInserts = true))
			{
				ciInsert(list, index + indexDelta, listToInsert);
				Debug.assert(indexDelta <= 1 || list.FindIndex(index + 1, indexDelta - 1, predicate) == -1, "indexDelta"); // just in case if indexDelta > 1

				index0 = index + indexIncrement; // next after finded or next after inserted

				if (--maxMatchCount == 0)
					break;

				listToInsert = copyCIList(listToInsert); // better make copy if need to insert this more than once (there might be a problems with labels otherwise)
			}

			Debug.assert(anyInserts, $"ciInsert: no insertions were made");
			Debug.assert(maxMatchCount <= 0, $"ciInsert: matchCount {maxMatchCount}");

			return list;
		}

		public static CIList ciInsert(CIList list, int index, CIList listToInsert)
		{
			if (index >= 0 && index <= list.Count)
			{
				List<Label> labels = null;

				if (index != list.Count && (list[index].labels?.Count ?? 0) > 0) // copy labels from instruction where we insert
				{
					labels = new List<Label>(list[index].labels);
					list[index].labels.Clear();
				}

				list.InsertRange(index, listToInsert);

				if (labels != null)
					list[index].labels.AddRange(labels); // add copied labels to a new instruction at 'index'
			}
			else Debug.assert(false, $"ciInsert: CodeInstruction index is invalid ({index})");

			return list;
		}
		#endregion

		#region ciRemove
		// indexDelta - change actual index from matched for removing
		// countToRemove - instructions count to be removed
		public static CIList ciRemove(CIEnumerable cins, CIPredicate predicate, int indexDelta, int countToRemove) =>
			ciRemove(cins.ToList(), predicate, indexDelta, countToRemove);

		public static CIList ciRemove(CIList list, CIPredicate predicate, int indexDelta, int countToRemove)
		{
			int index = list.FindIndex(predicate);
			return ciRemove(list, (index == -1? -1: index + indexDelta), countToRemove);
		}

		public static CIList ciRemove(CIEnumerable cins, int index, int countToRemove) =>
			ciRemove(cins.ToList(), index, countToRemove);

		public static CIList ciRemove(CIList list, int index, int countToRemove)
		{
			if (index >= 0 && index + countToRemove <= list.Count)
			{
				list.RemoveRange(index, countToRemove);
			}
			else Debug.assert(false, "ciRemove: CodeInstruction index is invalid");

			return list;
		}
		#endregion

		#region ciReplace
		// replaces first matched CodeInstruction with cinsForReplace CodeInstructions
		public static CIList ciReplace(CIEnumerable cins, CIPredicate predicate, params object[] cinsForReplace) =>
			ciReplace(cins.ToList(), predicate, cinsForReplace);

		public static CIList ciReplace(CIList list, CIPredicate predicate, params object[] cinsForReplace) =>
			ciReplace(list, list.FindIndex(predicate), cinsForReplace);

		public static CIList ciReplace(CIList list, int index, params object[] cinsForReplace)
		{
			if (index >= 0)
			{
				CIList listToInsert = toCIList(cinsForReplace);
				ciInsert(list, index, listToInsert); // insert first, so we can copy labels in ciInsert
				ciRemove(list, index + listToInsert.Count, 1);
			}
			else Debug.assert(false, "ciReplace: CodeInstruction index is invalid");

			return list;
		}
		#endregion
#endregion

#region LdcOpCode
		// helper class for getting LDC opcode based on number type
		// https://stackoverflow.com/questions/600978/how-to-do-template-specialization-in-c-sharp
		static class LdcOpCode
		{
			interface IGetOpCode<T> { OpCode get(); }

			class GetOpCode<T>: IGetOpCode<T>
			{
				class GetOpSpec: IGetOpCode<float>, IGetOpCode<double>, IGetOpCode<sbyte> // will add more when needed
				{
					public static readonly GetOpSpec S = new GetOpSpec();

					OpCode IGetOpCode<float>.get()  => OpCodes.Ldc_R4;
					OpCode IGetOpCode<double>.get() => OpCodes.Ldc_R8;
					OpCode IGetOpCode<sbyte>.get()  => OpCodes.Ldc_I4_S;
				}

				public static readonly IGetOpCode<T> S = GetOpSpec.S as IGetOpCode<T> ?? new GetOpCode<T>();

				OpCode IGetOpCode<T>.get() => OpCodes.Nop;
			}

			public static OpCode get<T>() => GetOpCode<T>.S.get();
		}
#endregion
	}
}