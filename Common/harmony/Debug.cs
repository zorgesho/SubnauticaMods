using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.Generic;

using Harmony;

namespace Common.Harmony
{
	using Reflection;

	static partial class HarmonyExtensions
	{
		public static void log(this CodeInstruction ci) => $"{ci.opcode} {ci.operand}".log();

		public static void log(this IEnumerable<CodeInstruction> cins, string filename = null, bool printIndexes = true, bool printFirst = false)
		{
			var sb = new StringBuilder();
			var list = cins.ToList();

			int _findLabel(object label) => // find target index for jumps
				list.FindIndex(_ci => _ci.labels?.FindIndex(l => l.Equals(label)) != -1);

			static string _label2Str(Label label) => $"Label{label.GetHashCode()}";

			static string _labelsInfo(CodeInstruction ci)
			{
				if ((ci.labels?.Count ?? 0) == 0)
					return "";

				string res = $" => labels({ci.labels.Count}): ";
				ci.labels.ForEach(l => res += _label2Str(l) + " ");

				return res;
			}

			for (int i = 0; i < list.Count; i++)
			{
				var ci = list[i];

				int labelIndex = (ci.operand is Label)? _findLabel(ci.operand): -1;
				string operandInfo = labelIndex == -1? ci.operand?.ToString(): $"jump to {(printIndexes? labelIndex.ToString(): "")}({_label2Str(ci.operand.cast<Label>())})";
				string isFirstOp = (printFirst && list.FindIndex(_ci => _ci.opcode == ci.opcode) == i)? " 1ST":""; // is such an opcode is first encountered in this instruction
				string prefix = printIndexes? $"{i:D3}{isFirstOp}: ": "";

				sb.AppendLine($"{prefix}{ci.opcode} {operandInfo}{_labelsInfo(ci)}");
			}

			if (filename == null)
			{
				sb.Insert(0, Environment.NewLine);
				sb.ToString().log();
			}
			else
			{
				sb.ToString().saveToFile(filename);
			}
		}
	}

	// searches types and methods in the assembly (including Common projects) for harmony attributes (from Harmony and HarmonyHelper)
	// and validates target method (do not runs prepare methods)
	// uncomment VALIDATE_PATCHES in HarmonyHelper.cs to run on start
	static class PatchesValidator
	{
		static bool testPassed;

		[Conditional("DEBUG")]
		public static void validate()
		{
			testPassed = true;

			Assembly.GetExecutingAssembly().GetTypes().forEach(type => checkType(type));

			if (testPassed)
				$"PatchesValidator: patches OK".logDbg();
		}

		static void checkType(Type type)
		{
			checkPatches(type);
			type.methods(ReflectionHelper.bfAll ^ BindingFlags.Instance).forEach(method => checkPatches(method));
		}

		static void checkPatches(MemberInfo member)
		{
			if (member.getAttr<HarmonyPatch>() is HarmonyPatch harmonyPatch && harmonyPatch.info.getTargetMethod() == null)
				_error($"{harmonyPatch.info.declaringType?.FullName}.{harmonyPatch.info.methodName}");

			if (member.getAttr<HarmonyHelper.PatchAttribute>() is HarmonyHelper.PatchAttribute patchAttr && patchAttr.targetMethod == null)
				_error($"{patchAttr.type?.FullName}.{patchAttr.methodName}");

			void _error(string method)
			{
				testPassed = false;
				$"PatchesValidator: target method for {member.fullName()} is not found! ({method})".logError();
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

				if (harmonyID != null && !patchInfo.Owners.Any(id => id.ToLower().Contains(harmonyID)))
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