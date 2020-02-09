using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
		public static BindingFlags bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		public static Type getCallingType() => new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().ReflectedType;

		// for getting mod's defined types, don't include any of Common projects types (or types without namespace)
		public static readonly List<Type> definedTypes =
			Assembly.GetExecutingAssembly().GetTypes().Where(type => !(type.Namespace?.StartsWith(nameof(Common)) ?? true)).ToList();

		public static MethodInfo safeGetMethod(string assemblyName, string typeName, string methodName)
		{
			try   { return Assembly.Load(assemblyName)?.GetType(typeName)?.method(methodName); }
			catch { return null; }
		}
	}


	static class TypeExtensions
	{
		static MethodInfo _method(this Type type, string name, Type[] types)
		{
			try { return types == null? type.GetMethod(name, ReflectionHelper.bfAll): type.GetMethod(name, types); }
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
		public static MethodInfo[] methods(this Type type, BindingFlags bf) => type.GetMethods(ReflectionHelper.bfAll | bf);
		public static PropertyInfo[] properties(this Type type) => type.GetProperties(ReflectionHelper.bfAll);

		public static A    getAttribute<A>(this MemberInfo memberInfo)   where A: Attribute => Attribute.GetCustomAttribute(memberInfo, typeof(A)) as A;
		public static bool checkAttribute<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.IsDefined(memberInfo, typeof(A));
	}


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

		public static T createDelegate<T>(this DynamicMethod dm) where T: class => dm.CreateDelegate(typeof(T)) as T;
	}


	static partial class StringExtensions
	{
		public static bool isNullOrEmpty(this string s) => (s == null || s == "");

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
}