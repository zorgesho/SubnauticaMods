using System;
using System.IO;
using System.Collections.Generic;

namespace Common
{
	static class MiscExtensions
	{
		public static void addRange<T>(this ICollection<T> target, IEnumerable<T> source) => source.forEach(e => target.Add(e));

		public static void add<T>(this List<T> target, T item, int count)
		{
			for (int i = 0; i < count; i++)
				target.Add(item);
		}

		public static void forEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence == null)
				return;

			var enumerator = sequence.GetEnumerator();
			while (enumerator.MoveNext())
				action(enumerator.Current);
		}
	}


	static class ArrayExtensions
	{
		public static T[] init<T>(this T[] array)
		{
			array.Initialize();
			return array;
		}

		public static int findIndex<T>(this T[] array, int beginIndex, int endIndex, Predicate<T> predicate) =>
			Array.FindIndex(array, beginIndex, endIndex - beginIndex, predicate);

		public static int findIndex<T>(this T[] array, Predicate<T> predicate) =>
			Array.FindIndex(array, predicate);

		public static T[] append<T>(this T[] array1, T[] array2)
		{
			if (array1 != null && (array2 == null || array2.Length == 0))
				return array1;

			if (array1 == null)
				return array2 ?? new T[0];

			T[] newArray = new T[array1.Length + array2.Length];

			array1.CopyTo(newArray, 0);
			array2.CopyTo(newArray, array1.Length);

			return newArray;
		}
	}


	static partial class StringExtensions
	{
		public static bool isNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

		public static string format(this string s, object arg0) => string.Format(s, arg0);
		public static string format(this string s, object arg0, object arg1) => string.Format(s, arg0, arg1);
		public static string format(this string s, object arg0, object arg1, object arg2) => string.Format(s, arg0, arg1, arg2);
		public static string format(this string s, params object[] args) => string.Format(s, args);

		public static string clampLength(this string s, int length)
		{
			if (length < 5 || s.Length <= length)
				return s;

			return s.Remove(length / 2, s.Length - length + 3).Insert(length / 2, "...");
		}

		static string formatFileName(string filename)
		{
			if (filename.isNullOrEmpty())
				return filename;

			if (Path.GetExtension(filename) == "")
				filename += ".txt";

			if (!Path.IsPathRooted(filename))
				filename = Paths.modRootPath + filename;

			return filename;
		}

		public static void saveToFile(this string s, string localPath)
		{
			try { File.WriteAllText(formatFileName(localPath), s); }
			catch (Exception e) { Log.msg(e); }
		}

		public static void appendToFile(this string s, string localPath)
		{
			try { File.AppendAllText(formatFileName(localPath), s + Environment.NewLine); }
			catch (Exception e) { Log.msg(e); }
		}
	}


	class UniqueIDs
	{
		readonly HashSet<string> allIDs = new HashSet<string>();
		readonly Dictionary<string, int> nonUniqueIDs = new Dictionary<string, int>();

		public bool ensureUniqueID(ref string id)
		{
			if (allIDs.Add(id)) // if this is new id, do nothing
				return true;

			nonUniqueIDs.TryGetValue(id, out int counter);
			nonUniqueIDs[id] = ++counter;

			id += "." + counter;
#if DEBUG
			$"UniqueIDs: fixed ID: {id}".logWarning();
#endif
			Debug.assert(allIDs.Add(id)); // checking updated id just in case

			return false;
		}
	}
}