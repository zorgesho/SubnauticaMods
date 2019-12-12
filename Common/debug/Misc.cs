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

		public static void startStopwatch()
		{
			nestLevel++;
			
			if (stopwatch == null)
				stopwatch = Stopwatch.StartNew();
		}

		public static double stopStopwatch(string msg, string filename = null)
		{
			if (--nestLevel > 0)
				return 0;

			stopwatch.Stop();
			
			double time = stopwatch.Elapsed.TotalMilliseconds;

			if (msg != null)
			{
				string result = $"{msg}: {time} ms";
				$"STOPWATCH: {result}".logDbg();

				if (filename != null)
				{
					if (Path.GetExtension(filename) == "")
						filename += ".prf";

					File.AppendAllText(Paths.modRootPath + filename, $"{result}{Environment.NewLine}");
				}
			}

			stopwatch = null;

			return time;
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
	}
}