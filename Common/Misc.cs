using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common
{
	class Tuple<T1, T2>
	{
		public T1 first  { get; private set; }
		public T2 second { get; private set; }
		public Tuple(T1 _first, T2 _second) { first = _first; second = _second; }
    }

	static class Tuple
	{
		public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second) => new Tuple<T1, T2>(first, second);
	}

	static class ObjectExtensions
	{
		public static int toInt(this object obj) => Convert.ToInt32(obj);
		public static bool toBool(this object obj) => Convert.ToBoolean(obj);
		public static float toFloat(this object obj) => Convert.ToSingle(obj);

		public static void setFieldValue(this object obj, FieldInfo field, object value)
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

		public static T[] init<T>(this T[] array) where T: new()
		{
			for (int i = 0; i < array.Length; ++i)
				array[i] = new T();
			return array;
		}
		
		public static int findIndex<T>(this T[] array, int beginIndex, int endIndex, Predicate<T> predicate)
		{
			return Array.FindIndex(array, beginIndex, endIndex - beginIndex, predicate);
		}
	}


	static partial class StringExtensions
	{
		public static void saveToFile(this string s, string localPath)
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
		public static void logStack(string msg = "")
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