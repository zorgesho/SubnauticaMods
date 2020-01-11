using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

namespace Common
{
	static partial class HarmonyHelper
	{
		#region CodeInstruction extensions

		public static bool isLDC<T>(this CodeInstruction ci, T val) => ci.isOp(OpCodeByType.get<T>(), val);

		public static bool isOp(this CodeInstruction ci, OpCode opcode, object operand = null) =>
			ci.opcode == opcode && (operand == null || ci.operand.Equals(operand));

		public static void log(this CodeInstruction ci) => $"{ci.opcode} {ci.operand}".log();

		public static void log(this IEnumerable<CodeInstruction> cins, bool searchFirstOps = false)
		{
			var list = cins.ToList();

			int _findLabel(object label) => // find target index for jumps
				list.FindIndex(_ci => _ci.labels?.FindIndex(l => l.Equals(label)) != -1);

			for (int i = 0; i < list.Count; i++)
			{
				var ci = list[i];

				int labelIndex = (ci.operand?.GetType() == typeof(Label))? _findLabel(ci.operand): -1;
				string operandInfo = labelIndex != -1? "jump to " + labelIndex: ci.operand?.ToString();

				string labelsInfo = ci.labels.Count > 0? "=> labels:" + ci.labels.Count: "";
				string isFirstOp = (searchFirstOps && list.FindIndex(_ci => _ci.opcode == ci.opcode) == i)? " 1ST":""; // is such an opcode is first encountered in this instruction

				$"{i}{isFirstOp}: {ci.opcode} {operandInfo} {labelsInfo}".log();
			}
		}
		#endregion

		// https://stackoverflow.com/questions/600978/how-to-do-template-specialization-in-c-sharp
		static class OpCodeByType
		{
			interface IGetOpCode<T> { OpCode get(); }

			class GetOpCode<T>: IGetOpCode<T>
			{
				class GetOpSpec: IGetOpCode<float>, IGetOpCode<double>, IGetOpCode<sbyte>
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
	}
}