using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common
{
	static class ObjectExtensions
	{
		static public int toInt(this object obj) => Convert.ToInt32(obj);
		static public bool toBool(this object obj) => Convert.ToBoolean(obj);
		static public float toFloat(this object obj) => Convert.ToSingle(obj);

		static public void setFieldValue(this object obj, FieldInfo field, object value)
		{
			try
			{
				if (field.FieldType.IsEnum)
					field.SetValue(obj, Convert.ChangeType(value, Enum.GetUnderlyingType(field.FieldType)));
				else
					field.SetValue(obj, Convert.ChangeType(value, field.FieldType));
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}
	}


	static class MiscExtensions
	{
		public static void forEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence != null)
			{
				var enumerator = sequence.GetEnumerator();
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}
	}


	static partial class StringExtensions
	{
		static public void saveToFile(this string s, string localPath)
		{
			try
			{
				if (localPath != null)
				{
					if (Path.GetExtension(localPath) == "")
						localPath += ".txt";
					
					File.WriteAllText(Paths.modRootPath + localPath, s);
				}
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}
	}


	static partial class Debug
	{
		// based on code from http://www.csharp-examples.net/reflection-callstack/
		static public void logStack(string msg = "")
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame[] stackFrames = stackTrace.GetFrames();

			StringBuilder output = new StringBuilder($"Callstack {msg}:{Environment.NewLine}");

			for (int i = 1; i < stackFrames.Length; ++i) // dont print first item, it is "printStack"
			{
				MethodBase method = stackFrames[i].GetMethod();
				output.AppendLine($"\t{method.DeclaringType.Name}.{method.Name}");
			}

			output.ToString().log();
		}
	}

	// TODO: move to separate file with logStack
	static partial class Debug
	{
		static Stopwatch stopwatch = null;
		static int nestLevel = 0;

		public static void startStopwatch()
		{
			++nestLevel;
			
			if (stopwatch == null)
				stopwatch = Stopwatch.StartNew();
		}
		
		public static void stopStopwatch(string msg, string filename = null)
		{
			if (--nestLevel == 0)
			{
				stopwatch.Stop();
				string result = $"{msg}: {stopwatch.Elapsed.TotalMilliseconds} ms";
				$"STOPWATCH: {result}".logDbg();

				if (filename != null)
				{
					if (Path.GetExtension(filename) == "")
						filename += ".prf";

					File.AppendAllText(Paths.modRootPath + filename, $"{result}{System.Environment.NewLine}");
				}

				stopwatch = null;
			}
		}
	}
}