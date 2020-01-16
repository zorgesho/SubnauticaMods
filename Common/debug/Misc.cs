using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Common
{
	static partial class Debug
	{
		static Stopwatch stopwatch = null;
		static int nestLevel = 0;

		// need that to avoid returning from stopStopwatch and be able to add Conditional attribute
		public static double stopwatchLastResult { get; private set; } = 0d;

		[Conditional("DEBUG")]
		public static void startStopwatch()
		{
			nestLevel++;

			if (stopwatch == null)
				stopwatch = Stopwatch.StartNew();
		}

		[Conditional("DEBUG")]
		public static void stopStopwatch(string msg = null, string filename = null)
		{
			if (--nestLevel > 0)
				return;

			stopwatch.Stop();
			stopwatchLastResult = stopwatch.Elapsed.TotalMilliseconds;
			stopwatch = null;

			if (msg == null)
				return;

			string result = $"{msg}: {stopwatchLastResult} ms";
			$"STOPWATCH: {result}".logDbg();

			if (filename != null)
			{
				if (Path.GetExtension(filename) == "")
					filename += ".prf";

				File.AppendAllText(Paths.modRootPath + filename, $"{result}{Environment.NewLine}");
			}
		}
	}


	static partial class Debug
	{
		// based on code from http://www.csharp-examples.net/reflection-callstack/
		public static void logStack(string msg = "")
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame[] stackFrames = stackTrace.GetFrames();

			StringBuilder output = new StringBuilder($"Callstack {msg}:{Environment.NewLine}");

			for (int i = 1; i < stackFrames.Length; i++) // dont print first item, it is "printStack"
			{
				MethodBase method = stackFrames[i].GetMethod();
				output.AppendLine($"\t{method.DeclaringType.Name}.{method.Name}");
			}

			output.ToString().log();
		}

		[Conditional("DEBUG")]
		public static void assert(bool condition, string message = null)
		{
			if (!condition)
			{
				string msg = $"Assertion failed: {message}";

				$"{msg}".logError();
				throw new Exception(msg);
			}
		}

		//public static MethodBase _findMethodInStack<A>(out A attribute) where A: Attribute
		//{
		//	attribute = null;

		//	foreach (var stackFrame in new StackTrace().GetFrames())
		//	{
		//		MethodBase method = stackFrame.GetMethod();

		//		if ((attribute = Attribute.GetCustomAttribute(method, typeof(A)) as A) != null)
		//			return method;
		//	}

		//	return null;
		//}
	}
}