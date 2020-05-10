using System;
using System.IO;
using System.Diagnostics;

namespace Common
{
	static partial class Debug
	{
		public static Profiler profiler(string message = null, string filename = null, bool allowNested = true) =>
#if DEBUG
			allowNested || Profiler.profilerCount == 0? new Profiler(message, filename): null;
#else
			null;
#endif

		public class Profiler: IDisposable
		{
			public static double lastResult { get; private set; }
#if DEBUG
			public static int profilerCount { get; private set; }

			readonly string message = null;
			readonly string filename = null;
			readonly Stopwatch stopwatch = null;
			readonly long mem = GC.GetTotalMemory(false);

			static string formatFileName(string filename)
			{
				if (filename.isNullOrEmpty())
					return filename;

				if (Path.GetExtension(filename) == "")
					filename += ".prf";

				return Paths.modRootPath + filename;
			}

			public Profiler(string message, string filename)
			{
				profilerCount++;

				this.message = message;
				this.filename = formatFileName(filename);

				stopwatch = Stopwatch.StartNew();
			}

			public void Dispose()
			{
				stopwatch.Stop();

				profilerCount--;
				assert(profilerCount >= 0);

				lastResult = stopwatch.Elapsed.TotalMilliseconds;

				if (message == null)
					return;

				long m = GC.GetTotalMemory(false) - mem;
				string memChange = $"{(m > 0? "+": "")}{(Math.Abs(m) > 1024L * 1024L? (m / 1024L / 1024L + "MB"): (m / 1024L + "KB"))} ({m})";

				string result = $"{message}: {lastResult} ms; mem alloc:{memChange}";
				$"PROFILER: {result}".log();

				if (filename != null)
					result.appendToFile(filename);
			}

			public static void _logCompare(double prevResult, string filename = null)
			{
				string res = $"PROFILER: DIFF {prevResult} ms -> {lastResult} ms, delta: {(lastResult - prevResult) / prevResult * 100f:F2} %";
				res.log();

				if (filename != null)
					res.appendToFile(formatFileName(filename));
			}
#else
			public Profiler(string _0, string _1) {}
			public void Dispose() {}
#endif
		}
	}
}