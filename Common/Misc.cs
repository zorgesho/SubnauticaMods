using System;
using System.IO;
using System.Collections.Generic;

namespace Common
{
	static class MiscExtensions
	{
		public static void addRange<T>(this ICollection<T> target, IEnumerable<T> source) => source.forEach(e => target.Add(e));

		public static void add<T>(this List<T> target, T item, int count) where T: struct
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
		public static T[] init<T>(this T[] array) where T: new()
		{
			for (int i = 0; i < array.Length; i++)
				array[i] = new T();
			return array;
		}

		public static bool isNullOrEmpty(this Array array) => array == null || array.Length == 0;

		public static bool contains<T>(this T[] array, T val) => Array.IndexOf(array, val) != -1;

		public static int findIndex<T>(this T[] array, int beginIndex, int endIndex, Predicate<T> predicate) =>
			Array.FindIndex(array, beginIndex, endIndex - beginIndex, predicate);

		public static int findIndex<T>(this T[] array, Predicate<T> predicate) =>
			Array.FindIndex(array, predicate);

		public static T[] append<T>(this T[] array1, T[] array2)
		{
			if (array1 != null && array2.isNullOrEmpty())
				return array1;

			if (array1 == null)
				return array2 ?? new T[0];

			T[] newArray = new T[array1.Length + array2.Length];

			array1.CopyTo(newArray, 0);
			array2.CopyTo(newArray, array1.Length);

			return newArray;
		}

		public static T[] subArray<T>(this T[] array, int indexBegin, int indexEnd = -1)
		{
			try
			{
				int length = (indexEnd == -1? array.Length - 1: indexEnd) - indexBegin + 1;
				T[] newArray = new T[length];

				Array.Copy(array, indexBegin, newArray, 0, length);
				return newArray;
			}
			catch (Exception e) { Log.msg(e); return null; }
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

		public static void saveToFile(this string s, string localPath)
		{
			try { File.WriteAllText(_formatFileName(localPath), s); }
			catch (Exception e) { Log.msg(e); }
		}

		public static void appendToFile(this string s, string localPath)
		{
			try { File.AppendAllText(_formatFileName(localPath), s + Environment.NewLine); }
			catch (Exception e) { Log.msg(e); }
		}

		static string _formatFileName(string filename) => Paths.formatFileName(filename, "txt");
	}


	class UniqueIDs
	{
		readonly HashSet<string> allIDs = new();
		readonly Dictionary<string, int> nonUniqueIDs = new();

		public bool ensureUniqueID(ref string id, bool nonUniqueIDsWarning = true)
		{
			if (allIDs.Add(id)) // if this is new id, do nothing
				return true;

			nonUniqueIDs.TryGetValue(id, out int counter);
			nonUniqueIDs[id] = ++counter;

			id += "." + counter;
#if DEBUG
			if (nonUniqueIDsWarning)
				$"UniqueIDs: fixed ID: {id}".logWarning();

			Debug.assert(allIDs.Add(id)); // checking updated id just in case
#endif
			return false;
		}

		public bool freeID(string id) => allIDs.Remove(id); // non-unique IDs can't be freed (? for now)
	}
}