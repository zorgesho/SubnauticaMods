using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;

namespace Common
{
	static class ObjectExtensions
	{
		public static int   toInt(this object obj)   => Convert.ToInt32(obj, CultureInfo.InvariantCulture);
		public static bool  toBool(this object obj)  => Convert.ToBoolean(obj, CultureInfo.InvariantCulture);
		public static float toFloat(this object obj) => Convert.ToSingle(obj, CultureInfo.InvariantCulture);

		public static void setFieldValue(this object obj, FieldInfo field, object value)
		{
			try
			{
				if (field.FieldType.IsEnum)
					field.SetValue(obj, Convert.ChangeType(value, Enum.GetUnderlyingType(field.FieldType), CultureInfo.InvariantCulture));
				else
					field.SetValue(obj, Convert.ChangeType(value, field.FieldType, CultureInfo.InvariantCulture));
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}
	}

	static class ReflectionHelper
	{
		// for getting mod's defined types, don't return any of Common projects types (or types without namespace)
		public static IEnumerable<Type> definedTypes
		{
			get => Assembly.GetExecutingAssembly().GetTypes().Where(type => !(type.Namespace?.StartsWith(nameof(Common)) ?? true));
		}

		public static Type getCallingType() => new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().ReflectedType;

		public static BindingFlags bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	}

	static class TypeExtensions
	{
		static MethodInfo _method(this Type type, string name, Type[] types)
		{
			try
			{
				return types == null? type.GetMethod(name, ReflectionHelper.bfAll): type.GetMethod(name, types);
			}
			catch (AmbiguousMatchException)
			{
				$"Ambiguous method: {type.Name}.{name}".logError();
				return null;
			}
		}

		public static MethodInfo method(this Type type, string name) => _method(type, name, null);
		public static MethodInfo method(this Type type, string name, params Type[] types) => _method(type, name, types);

		public static FieldInfo field(this Type type, string name) => type.GetField(name, ReflectionHelper.bfAll);

		public static FieldInfo[] fields(this Type type) => type.GetFields(ReflectionHelper.bfAll);
		public static MethodInfo[] methods(this Type type) => type.GetMethods(ReflectionHelper.bfAll);
		public static PropertyInfo[] properties(this Type type) => type.GetProperties(ReflectionHelper.bfAll);
	}

	static class MiscExtensions
	{
		public static void add<T>(this List<T> target, T item, int count)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));
			if (item == null)   throw new ArgumentNullException(nameof(item));

			for (int i = 0; i < count; i++)
				target.Add(item);
		}

		public static void addRange<T>(this ICollection<T> target, IEnumerable<T> source)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));
			if (source == null) throw new ArgumentNullException(nameof(source));

			source.forEach(e => target.Add(e));
		}

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
			for (int i = 0; i < array.Length; i++)
				array[i] = new T();
			return array;
		}

		public static int findIndex<T>(this T[] array, int beginIndex, int endIndex, Predicate<T> predicate) =>
			Array.FindIndex(array, beginIndex, endIndex - beginIndex, predicate);

		public static int findIndex<T>(this T[] array, Predicate<T> predicate) =>
			Array.FindIndex(array, predicate);
	}


	static partial class StringExtensions
	{
		public static bool isNullOrEmpty(this string s) => (s == null || s == "");

		public static void saveToFile(this string s, string localPath)
		{
			try
			{
				if (localPath.isNullOrEmpty())
					return;

				if (Path.GetExtension(localPath) == "")
					localPath += ".txt";

				if (!Path.IsPathRooted(localPath))
					localPath = Paths.modRootPath + localPath;

				File.WriteAllText(localPath, s);
			}
			catch (Exception e)
			{
				Log.msg(e);
			}
		}
	}
}