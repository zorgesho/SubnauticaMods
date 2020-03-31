using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using Harmony;

namespace Common
{
	static partial class HarmonyHelper
	{
		// produces a list of all methods patched by all Harmony instances and their respective patches
		public static string getPatchesReport(string harmonyID = null, bool omitNames = false) => PatchesReport.get(harmonyID, omitNames);

		static class PatchesReport
		{
			public static string get(string harmonyID, bool omitNames)
			{
				Debug.assert(harmonyInstance != null, "Harmony is not initialized");

				var patchedMethods = harmonyInstance.GetPatchedMethods().ToList();
				patchedMethods.Sort((m1, m2) => string.Compare(m1.fullName(), m2.fullName(), StringComparison.Ordinal));

				var sb = new StringBuilder();
				harmonyID = harmonyID?.ToLower();

				foreach (var method in patchedMethods)
				{
					var patchInfo = harmonyInstance.GetPatchInfo(method); // that's bottleneck

					if (harmonyID != null && patchInfo.Owners.FirstOrDefault(id => id.ToLower().Contains(harmonyID)) == null)
						continue;

					appendMethodInfo(method, patchInfo, sb, omitNames);
				}

				return sb.ToString();
			}


			static void appendMethodInfo(MethodBase method, Patches patchInfo, StringBuilder sb, bool omitNames)
			{
				int patchCount = patchInfo.Prefixes.Count + patchInfo.Postfixes.Count + patchInfo.Transpilers.Count;

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
}