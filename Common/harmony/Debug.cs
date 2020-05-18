using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

namespace Common.Harmony
{
	using Reflection;

	static partial class HarmonyExtensions
	{
		public static void log(this CodeInstruction ci) => $"{ci.opcode} {ci.operand}".log();

		public static void log(this IEnumerable<CodeInstruction> cins, bool searchFirstOps = false)
		{
			var list = cins.ToList();

			int _findLabel(object label) => // find target index for jumps
				list.FindIndex(_ci => _ci.labels?.FindIndex(l => l.Equals(label)) != -1);

			static string _labelsInfo(CodeInstruction ci)
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
	}

	// produces a list of all methods patched by all Harmony instances and their respective patches
	static class PatchesReport
	{
		public static string get(string harmonyID = null, bool omitNames = false)
		{
			var patchedMethods = HarmonyHelper.harmonyInstance.GetPatchedMethods().ToList();
			patchedMethods.Sort((m1, m2) => string.Compare(m1.fullName(), m2.fullName(), StringComparison.Ordinal));

			var sb = new StringBuilder();
			harmonyID = harmonyID?.ToLower();

			foreach (var method in patchedMethods)
			{
				var patchInfo = HarmonyHelper.getPatchInfo(method); // that's bottleneck

				if (harmonyID != null && patchInfo.Owners.FirstOrDefault(id => id.ToLower().Contains(harmonyID)) == null)
					continue;

				appendMethodInfo(method, patchInfo, sb, omitNames);
			}

			return sb.ToString();
		}


		static void appendMethodInfo(MethodBase method, Patches patchInfo, StringBuilder sb, bool omitNames)
		{
			int patchCount = patchInfo.Prefixes.Count + patchInfo.Postfixes.Count + patchInfo.Transpilers.Count;

			if (patchCount == 0) // it can be zero if we first patch and then unpatch method
				return;

			sb.Append($"{method.fullName()}:");
			if (patchCount > 1)
				sb.AppendLine();

			_appendPatches("PREFIX", patchInfo.Prefixes);
			_appendPatches("POSTFIX", patchInfo.Postfixes);
			_appendPatches("TRANSPILER", patchInfo.Transpilers);

			void _appendPatches(string patchType, IList<Patch> patches)
			{
				if (patches.Count == 0)
					return;

				var sortedPatches = patches.ToList();
				sortedPatches.Sort(); // sort patches in the same order they execute

				foreach (var patch in sortedPatches)
				{
					sb.Append($"{(patchCount > 1?"\t":" ")}{patchType}: ({patch.owner}) ");

					if (patch.priority != Priority.Normal)
						sb.Append($"[{patch.priority}] ");

					if (!omitNames)
						sb.Append(patch.patch.fullName());

					sb.AppendLine();
				}
			}
		}
	}
}