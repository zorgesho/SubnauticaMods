using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices // nice trick (for use in .NET 4.0)
{
	// https://thomaslevesque.com/2012/06/13/using-c-5-caller-info-attributes-when-targeting-earlier-versions-of-the-net-framework/

	[AttributeUsage(AttributeTargets.Parameter)] class CallerFilePathAttribute: Attribute {}
	[AttributeUsage(AttributeTargets.Parameter)] class CallerMemberNameAttribute: Attribute {}
	[AttributeUsage(AttributeTargets.Parameter)] class CallerLineNumberAttribute: Attribute {}
}

namespace Common
{
	static partial class Debug
	{
		public class Profiler: IDisposable
		{
			public static double lastResult { get; private set; }
#if DEBUG
			public static int profilersCount { get; private set; }

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
				profilersCount++;

				this.message = message;
				this.filename = formatFileName(filename);

				stopwatch = Stopwatch.StartNew();
			}

			public void Dispose()
			{
				stopwatch.Stop();

				profilersCount--;
				assert(profilersCount >= 0);

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

		public static Profiler profiler(string message = null, string filename = null, bool allowNested = true) =>
#if DEBUG
			allowNested || Profiler.profilersCount == 0? new Profiler(message, filename): null;
#else
			null;
#endif
	}


	static partial class Debug
	{
		// based on code from http://www.csharp-examples.net/reflection-callstack/
		public static void logStack(string msg = "")
		{
			var stackFrames = new StackTrace().GetFrames();
			var sb = new StringBuilder($"Callstack {msg}:{Environment.NewLine}");

			for (int i = 1; i < stackFrames.Length; i++) // don't print first item, it is "logStack"
				sb.AppendLine($"\t{stackFrames[i].GetMethod().fullName()}");

			sb.ToString().log();
		}

		[Conditional("DEBUG")]
		public static void assert(bool condition, string message = null, [CallerFilePath] string __filename = "", [CallerLineNumber] int __line = 0)
		{
			if (condition)
				return;

			string msg = "Assertion failed" + (message != null? $": {message}": "") + $" ({__filename}:{__line})";

			$"{msg}".logError();
			throw new Exception(msg);
		}
	}
}